using ProdmasterSalaryService.Models.Classes;

namespace ProdmasterSalaryService.Services.Interfaces
{
    public interface IOperationService
    {
        Task<Operation[]> GetOperations();
        Task<Operation[]> GetOperationsByUser(User user);
        Task<Operation[]> GetOperationsByCustom(Custom custom);
    }
}
