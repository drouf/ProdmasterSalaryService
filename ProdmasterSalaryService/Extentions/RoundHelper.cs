using ProdmasterSalaryService.ViewModels.Report;

namespace ProdmasterSalaryService.Extentions
{
    public static class RoundHelper
    {
        public static double Round(this double value, int symb) {  return Math.Round(value, symb); }

        public static void RoundAllDoubles(this ReportModel model, int symb)
        {
            var props = model.GetType().GetProperties().Where(p => p.GetValue(model).GetType() == typeof(double)).ToArray();
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                if (value != null)
                {
                    var doubleValue = (double)value;
                    prop.SetValue(model, doubleValue.Round(symb));
                }
            }
        }
    }
}
