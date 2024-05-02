using FluentValidation;
using MediatR;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.ViewModels.Report;

namespace ProdmasterSalaryService.Requests
{
    public class ReportIndexRequest : IRequest<ReportModel>
    {
        public int? Year {  get; set; }
        public int? Month { get; set; }
        public User? User { get; set; }
    }

    public class ReportIndexValidator : AbstractValidator<ReportIndexRequest>
    {
        public ReportIndexValidator()
        {
            RuleFor(c => c.Year)
                .Must(c => (c > 1990 && c <= DateTime.Now.Year) || c == null)
                .WithMessage("Год должен быть в промежутке между 1990 и текущим");

            RuleFor(c => c.Month)
                .Must(c => (c >= 1 && c <= 12) || c == null)
                .WithMessage("Месяц должен быть в промежутке между 0 и 11");
        }
    }
}
