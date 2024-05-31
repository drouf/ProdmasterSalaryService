using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Extentions;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Requests;
using ProdmasterSalaryService.ViewModels.Report;
using System;
using System.Linq;
using System.Net.WebSockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProdmasterSalaryService.Handlers
{
    public class ReportIndexHandler : IRequestHandler<ReportIndexRequest, ReportModel>
    {
        private readonly UserContext _dbContext;
        public ReportIndexHandler(UserContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ReportModel> Handle(ReportIndexRequest request, CancellationToken cancellationToken)
        {
            if (request.Year == null) { request.Year = DateTime.Now.Year; }
            if (request.Month == null)
            {
                DateTime current;
                if (DateTime.Now.Day < 25)
                {
                    current = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    current = DateTime.Now;
                }
                request.Year = current.Year;
                request.Month = current.Month;
            }
            try
            {
                return await GetReportModelAsync(request.User, (int)request.Year, (int)request.Month);
            }
            catch(Exception exception)
            {
                return new ReportModel();
            }
        }

        private async Task<ReportModel> GetReportModelAsync(User user, int year, int month)
        {
            var draft = await GetReportDraft(user, year, month);

            int workedDays = draft.DayDrafts.Where(d => d.Shift != null && d.Shift.Coefficient == 1 && d.Shift.DisanId > 0).Count();
            int willWorkDays = draft.DayDrafts.Where(d => d.DayOfWeek <= 4 && d.Date > DateTime.Now.Date).Count();

            var report = new ReportModel()
            {
                Salary = (user.Custom != null) ? user.Custom.Salary : 0,
                Title = ($"{draft.DayDrafts.First().Date:MMMM} {year}").ToUpper(),
                WorkDays = draft.DayDrafts.Where(d => (d.DayOfWeek <= 4 || d.Shift != null) && !(d.Shift != null
                        && d.Shift.Coefficient == 1
                        && d.Shift.DisanId < 0)).Count(),
                SkipedDays = draft.DayDrafts.Where(d => d.CssClassName.Equals("passed-day")).Count(),    
            };

            report.RemoteDays = draft.DayDrafts.Where(d => d.Shift != null && d.Shift.Coefficient == 0.5).Count();
            report.SalaryPerDay = report.Salary / report.WorkDays;
            report.HowMuchWillGet = ((workedDays + willWorkDays) * report.SalaryPerDay) + (report.RemoteDays * report.SalaryPerDay * 0.5);
            report.HowMuchLose = report.Salary - report.HowMuchWillGet;

            var operations = GetOperationsForReport(user, year, month);
            report.Operations = await operations.ToListAsync();
            report.HowMuchGet = await operations.SumAsync(o => o.Sum);

            if (report.HowMuchLeftToGet < 0)
            {
                report.Bonus = -1 * report.HowMuchLeftToGet;
                report.HowMuchLeftToGet = 0;
            }

            var curRow = 1;
            foreach(var date in draft.DayDrafts)
            {
                report.SetField(curRow, date.DayOfWeek, date.Date.Day.ToString(), date.CssClassName);
                if (date.DayOfWeek == 6)
                {
                    curRow++;
                }
            }

            report.RoundAllDoubles(2);

            return report;
        }

        private IQueryable<Operation> GetOperationsForReport(User user, int year, int month)
        {
            var operations = _dbContext.Set<Operation>().AsNoTracking().Where(o => o.Custom != null && user.Custom != null  && o.Custom.Id == user.Custom.Id);
            var firstDay = new DateTime(year, month, 1);
            var currentMonthName = firstDay.ToString("MMMM").ToLower();
            var prevMonthName = firstDay.AddDays(-1).ToString("MMMM").ToLower();
            operations = operations.Where(o => (o.Paid.Year == year && o.Paid.Month == month && o.Note != null && !(o.Note.ToLower().Contains(prevMonthName))) 
                || (o.Note != null && o.Note.ToLower().Contains(currentMonthName) && o.Paid.Year == ((month < 12) ? year : year + 1)))
                .OrderBy(o => o.Date);
            return operations;
        }

        private int GetDayOfWeekThisCulture(DayOfWeek day)
        {
            return (day > 0) ? (int)day - 1 : 6;
        }
        private async Task<ReportDraft> GetReportDraft(User user, int year, int month)
        {
            var shifts = await _dbContext.Set<Shift>()
                        .AsNoTracking()
                        .Where(s =>
                            s.Start != null
                            && s.Custom != null
                            && user.Custom != null
                            && s.Custom.Id == user.Custom.Id
                            && s.Start.Value.Year == year
                            && s.Start.Value.Month == month)
                        .ToListAsync();

            var reportDraft = new ReportDraft()
            {
                DayDrafts = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => 
                    new DayDraft() 
                    {
                        Date = new DateTime(year, month, day) 
                    })
                .ToList()
            };

            foreach(var day in reportDraft.DayDrafts)
            {

                day.DayOfWeek = GetDayOfWeekThisCulture(day.Date.DayOfWeek);
                day.Shift = shifts.FirstOrDefault(y => y.Start!.Value.Day == day.Date.Day);
                day.CssClassName = GetClassForDay(day.Date, day.Shift);
            }

            return reportDraft;
        }
        private string GetClassForDay(DateTime day, Shift? shift)
        {
            if (shift == null)
            {
                if (GetDayOfWeekThisCulture(day.DayOfWeek) > 4)
                {
                    return "weekend-day";
                }
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
                {
                    if (shift.DisanId > 0)
                    {
                        return "work-day";
                    }
                    else
                    {
                        return "holiday-day";
                    }
                }
                else
                {
                    return "remote-day";
                }
            }
        }
    }
}
