namespace ProdmasterSalaryService.Models.Interfaces
{
    public interface IDisanModel
    {
        long DisanId { get; set; }
        DateTime? Created { get; set; }
        DateTime? Modified { get; set; }
    }
}
