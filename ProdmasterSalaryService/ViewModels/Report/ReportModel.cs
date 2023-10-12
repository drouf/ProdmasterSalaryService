using Microsoft.AspNetCore.SignalR;
using ProdmasterSalaryService.Models.Classes;
using static Npgsql.PostgresTypes.PostgresCompositeType;

namespace ProdmasterSalaryService.ViewModels.Report
{
    public class ReportModel
    {
        public List<ReportRow> Fields { get; set; } = 
            new List<ReportRow>() { 
                new ReportRow() { Row = 
                        new List<ReportField>() { 
                        new ReportField() { Field = "ПН" },
                        new ReportField() { Field = "ВТ" },
                        new ReportField() { Field = "СР" },
                        new ReportField() { Field = "ЧТ" },
                        new ReportField() { Field = "ПТ" },
                        new ReportField() { Field = "СБ" },
                        new ReportField() { Field = "ВС" },
                    } 
                } 
            };
        public string Title { get; set; } = "";
        public int WorkDays { get; set; } = 0;
        public double Salary { get; set; } = 0;
        public double SalaryPerDay { get; set; } = 0;
        public int SkipedDays { get; set; } = 0;
        public double HowMuchWillGet { get; set; } = 0;
        public double HowMuchLose { get; set; } = 0;
        public double HowMuchGet {  get; set; } = 0;
        public double HowMuchLeftToGet {  get; set; } = 0;
        public double Bonus { get; set; } = 0;

        public List<Models.Classes.Operation> Operations { get; set; } = new List<Models.Classes.Operation>();

        public string? GetField(int row, int col)
        {
            try
            {
                return Fields[index: row].Row[index: col].Field;
            }
            catch (Exception) 
            {
                return null;
            }
        }

        public void SetField(int row, int col, string value, string className = "common-day")
        {
            if (col < 0 || col > 6) { return; }

            if (Fields.Count <= row)
            {
                for(int i = Fields.Count; i <= row; i++)
                {
                    Fields.Add(new ReportRow());
                }
            }

            Fields[index: row].Row[index: col].Field = value;
            Fields[index: row].Row[index: col].Class = className;
        }
    }
}