
using Microsoft.AspNetCore.Authorization;
using ProdmasterSalaryService.Extentions;
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
using ProdmasterSalaryService.Requests;
using MediatR;

namespace ProdmasterSalaryService.Controllers
{
    [Route("report")]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IUserService _userService;
        private readonly IShiftService _shiftsService;
        private readonly IMediator _mediator;
        public ReportController(ILogger<ReportController> logger, IOperationService operationService, IUserService userService, IShiftService shiftsService, IMediator mediator)
        {
            _logger = logger;
            _operationService = operationService;
            _userService = userService;
            _shiftsService = shiftsService;
            _mediator = mediator;
        }

        [HttpGet]

        public async Task<IActionResult> Index([FromQuery] ReportIndexRequest request)
        {
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            request.User = user;

            try
            {
                var model = await _mediator.Send(request);
                await FillSelectsViewBag(user);
                return View(model);
            }
            catch(FluentValidation.ValidationException validationException)
            {
                return BadRequest(validationException.Errors);
            }
        }

        [HttpGet]
        [Route("refreshReportsTable")]
        public async Task<IActionResult> RefreshReportTable([FromQuery] ReportIndexRequest request)
        {
            var user = await GetUser();

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }

            request.User = user;

            try
            {
                var model = await _mediator.Send(request);
                return PartialView("_ReportTable", model);
            }
            catch (FluentValidation.ValidationException validationException)
            {
                return BadRequest(validationException.Errors);
            }
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
                model.MenuItems.Add(new ReportMenuItemModel() { Title = "Пометить как праздник", OnClick = "MarkAsHoliday()" });
            }
            if(shift != null)
            {
                if (shift.Coefficient == 0.5 || (shift.Coefficient == 1 && shift.DisanId < 0))
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

        [HttpGet]
        [Route("setHoliday")]
        public async Task<IActionResult> SetHoliday(string dateString)
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
                if (!await _shiftsService.AddHolidayShift(date, user))
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

        [NonAction]
        private Task<User> GetUser()
        {
            var userLogin = User.Identity?.Name;
            return _userService.GetByLogin(userLogin);
        }

        [NonAction]
        private async Task FillSelectsViewBag(User user)
        {
            var selectYear = new Dictionary<int, string>();
            var firstShift = await _shiftsService.GetFirstWorkersShiftOrDefault(user);
            if(firstShift != null && firstShift.Start!= null && firstShift.Start.Value.Year != DateTime.Now.Year)
            {
                for (var year = firstShift.Start.Value.Year; year < DateTime.Now.Year; year++)
                {
                    selectYear.Add(year, year.ToString());
                }
            }
            selectYear.Add(DateTime.Now.Year, (DateTime.Now.Year).ToString());

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
