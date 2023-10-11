using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;

namespace ProdmasterSalaryService.Services.Classes
{
    public class OperationService : IOperationService
    {
        private readonly OperationRepository _repository;
        public OperationService(OperationRepository repository)
        {
            _repository = repository;
        }
        public Task<Operation[]> GetOperations()
        {
            return _repository.Select();
        }

        public Task<Operation[]> GetOperationsByUser(User user)
        {
            return _repository.Where(o => o.Custom.User.Id == user.Id);
        }

        public Task<Operation[]> GetOperationsByCustom(Custom custom)
        {
            return _repository.Where(o => o.Custom.Id == custom.Id);
        }
    }
}
