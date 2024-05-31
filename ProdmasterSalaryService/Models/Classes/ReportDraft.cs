namespace ProdmasterSalaryService.Models.Classes
{
    public class ReportDraft
    {
        public List<DayDraft> DayDrafts { get; set; } = new List<DayDraft>();
    }

    public class DayDraft
    {
        public DateTime Date { get; set; }
        public string CssClassName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public Shift? Shift { get; set; }
    }
}
