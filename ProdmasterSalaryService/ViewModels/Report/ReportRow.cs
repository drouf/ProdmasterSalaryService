using System.Collections;

namespace ProdmasterSalaryService.ViewModels.Report
{
    public class ReportRow
    {
        public List<ReportField> Row { get; set; } = new List<ReportField>() { 
            new ReportField(), 
            new ReportField(), 
            new ReportField(), 
            new ReportField(), 
            new ReportField(), 
            new ReportField(), 
            new ReportField(),
        };
    }
}
