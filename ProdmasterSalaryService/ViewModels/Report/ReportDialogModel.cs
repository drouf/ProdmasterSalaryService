namespace ProdmasterSalaryService.ViewModels.Report
{
    public class ReportDialogModel
    {
        public DateTime Date { get; set; }
        public List<ReportMenuItemModel> MenuItems { get; set; } = new List<ReportMenuItemModel>();
    }
}
