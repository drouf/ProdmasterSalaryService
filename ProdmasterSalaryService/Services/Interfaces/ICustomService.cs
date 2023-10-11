using ProdmasterSalaryService.Models.Classes;

namespace ProdmasterSalaryService.Services.Interfaces
{
    public interface ICustomService
    {
        Task<Custom[]> GetCustoms();
        Task<Custom> GetByDisanId(long disanId);
    }
}
