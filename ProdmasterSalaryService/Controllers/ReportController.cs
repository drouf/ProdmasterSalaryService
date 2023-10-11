
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Services.Classes;
using ProdmasterSalaryService.Services.Interfaces;
using ProdmasterSalaryService.ViewModels.Operation;
using ProdmasterSalaryService.ViewModels.Report;
using System;
using System.Globalization;
using System.Linq;

namespace ProdmasterSalaryService.Controllers
{
    [Route("report")]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IUserService _userService;
        private readonly IOperationService _operationService;
        private readonly IShiftService _shiftsService;
        public ReportController(ILogger<ReportController> logger, IOperationService operationService, IUserService userService, IShiftService shiftsService)
        {
            _logger = logger;
            _operationService = operationService;
            _userService = userService;
            _shiftsService = shiftsService;
        }

        [HttpGet]

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            if (year == null) { year =  DateTime.Now.Year; }
            if (month == null) { month = DateTime.Now.Month; }

            await FillSelectsViewBag();

            return View(await GetReportModelAsync(user, (int)year, (int)month));
        }

        [HttpGet]
        [Route("refreshReportsTable")]
        public async Task<IActionResult> RefreshReportTable(int? year, int? month)
        {
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            if (year == null) { year = DateTime.Now.Year; }
            if (month == null) { month = DateTime.Now.Month; }            

            return PartialView("_ReportTable", await GetReportModelAsync(user, (int)year, (int)month));
        }

        [HttpGet]
        [Route("getMenuForDay")]
        public async Task<IActionResult> GetMenu(string dateString)
        {
            if (dateString == null || dateString == string.Empty) { return BadRequest("Failed to get date"); }
            var success = DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date);
            if (!success) { return BadRequest("Failed to parse date"); }
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            var shift = await _shiftsService.FirstByDateAsync(date, user);
            var model = new ReportDialogModel() { Date = date, MenuItems = new List<ReportMenuItemModel>() };

            if (shift == null && date.Date <= DateTime.Now.Date)
            {
                model.MenuItems.Add(new ReportMenuItemModel() { Title = "Пометить как удаленка", OnClick = "MarkAsRemote()" });
            }
            if(shift != null)
            {
                if (shift.Coefficient == 0.5)
                {
                    model.MenuItems.Add(new ReportMenuItemModel() { Title = "Пометить как пропуск", OnClick = "MarkAsSkip()" });
                }
            }
            model.MenuItems.Add(new ReportMenuItemModel() { Title = "Отмена", OnClick = "CloseModal()" });
            return PartialView("_ReportDialog", model);
        }

        [HttpGet]
        [Route("setDateRemote")]
        public async Task<IActionResult> SetDateRemote(string dateString)
        {
            if (dateString == null || dateString == string.Empty) { return BadRequest("Failed to get date"); }
            var success = DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date);
            if (!success) { return BadRequest("Failed to parse date"); }
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            var shift = await _shiftsService.FirstByDateAsync(date, user);
            if (shift == null)
            {
                if(!await _shiftsService.AddRemoteShift(date, user))
                {
                    return BadRequest("Failed to add remote shift");
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return BadRequest("Shift exists");
            }
        }

        [HttpGet]
        [Route("setDateSkip")]
        public async Task<IActionResult> SetDateSkip(string dateString)
        {
            if (dateString == null || dateString == string.Empty) { return BadRequest("Failed to get date"); }
            var success = DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date);
            if (!success) { return BadRequest("Failed to parse date"); }
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            var shift = await _shiftsService.FirstByDateAsync(date, user);
            if (shift == null)
            {
                return BadRequest("Shift not exists");
            }
            else
            {
                if(!await _shiftsService.RemoveRemoteShift(date, user))
                {
                    return BadRequest("Shift not exists");
                }
                else
                {
                    return Ok();
                }
            }
        }

        [NonAction]
        private async Task<ReportModel> GetReportModelAsync(User user, int year, int month)
        {
            try
            {
                var reportModel = new ReportModel();
                reportModel.Salary = (user.Custom != null) ? user.Custom.Salary : 0;
                var curRow = 1;
                var firstDay = new DateTime((int)year, (int)month, 1);
                var lastDay = new DateTime((int)year, (int)month, DateTime.DaysInMonth((int)year, (int)month));
                reportModel.Title = ($"{firstDay.ToString("MMMM")} {year}").ToUpper();

                int workedDays = 0;
                int remoteDays = 0;

                for (DateTime day = firstDay; day <= lastDay; day = day.AddDays(1))
                {
                    var dayOfWeek = GetDayOfWeekThisCulture(day.DayOfWeek);

                    if(dayOfWeek <= 4) { reportModel.WorkDays += 1; }

                    var shift = await _shiftsService.FirstByDateAsync(day, user);

                    if(shift != null)
                    {
                        if (shift.Coefficient == 1) { workedDays += 1; }
                        if (shift.Coefficient == 0.5) { remoteDays += 1; }
                    }

                    var classForDay = await GetClassForDayAsync(day, user);

                    if (classForDay == "passed-day")
                        reportModel.SkipedDays += 1;

                    reportModel.SetField(curRow, dayOfWeek, day.Day.ToString(), classForDay);

                    if (dayOfWeek == 6)
                    {
                        curRow++;
                    }
                }

                reportModel.SalaryPerDay = Math.Round(reportModel.Salary / reportModel.WorkDays, 2);
                reportModel.HowMuchWillGet = Math.Round((workedDays * reportModel.SalaryPerDay) + (remoteDays * reportModel.SalaryPerDay * 0.5), 2);
                reportModel.HowMuchLose = Math.Round(reportModel.Salary - reportModel.HowMuchWillGet, 2);

                var operations = (await _operationService.GetOperationsByUser(user)).ToList();
                operations = operations.Where(o => o.Note != null && o.Note.ToLower().Contains(firstDay.ToString("MMMM").ToLower()) && o.Paid.Year == year).ToList();

                reportModel.Operations = operations;
                reportModel.HowMuchGet = Math.Round(operations.Sum(o => o.Sum),2);
                reportModel.HowMuchLeftToGet = Math.Round(reportModel.HowMuchWillGet - reportModel.HowMuchGet, 2);

                return reportModel;
            }
            catch (Exception)
            {
                return new ReportModel();
            }
        }

        [NonAction]
        private Task<User> GetUser()
        {
            var userLogin = User.Identity?.Name;
            return _userService.GetByLogin(userLogin);
        }

        [NonAction]
        private int GetDayOfWeekThisCulture(DayOfWeek day)
        {
            return (day > 0) ? (int)day - 1 : 6;
        }

        private async Task<string> GetClassForDayAsync(DateTime day, User user)
        {
            if (GetDayOfWeekThisCulture(day.DayOfWeek) > 4)
            {
                return "weekend-day";
            }

            var shift = await _shiftsService.FirstByDateAsync(day, user);
            if(shift == null)
            {
                if (day > DateTime.Now.Date)
                {
                    return "future-day";
                }
                else
                {
                    return "passed-day";
                }
            }
            else
            {
                if (shift.Coefficient == 1)
                    return "work-day";
                else
                    return "remote-day";
            }
        }

        [NonAction]
        private async Task FillSelectsViewBag()
        {
            var selectYear = new Dictionary<int, string>()
            {
                {DateTime.Now.Year-1, (DateTime.Now.Year-1).ToString()},
                {DateTime.Now.Year, (DateTime.Now.Year).ToString()},
                {DateTime.Now.Year+1, (DateTime.Now.Year+1).ToString()},
            };

            var selectMonth = new Dictionary<int, string>()
            {
                {1, "Январь"},
                {2, "Февраль"},
                {3, "Март"},
                {4, "Апрель"},
                {5, "Май"},
                {6, "Июнь"},
                {7, "Июль"},
                {8, "Август"},
                {9, "Сентябрь"},
                {10, "Октябрь"},
                {11, "Ноябрь"},
                {12, "Декабрь"},
            };

            ViewBag.Year = selectYear;
            ViewBag.Month = selectMonth;
        }
    }
}
