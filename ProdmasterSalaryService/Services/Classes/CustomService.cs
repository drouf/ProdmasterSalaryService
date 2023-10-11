using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;

namespace ProdmasterSalaryService.Services.Classes
{
    public class CustomService : ICustomService
    {
        private readonly CustomRepository _repository;
        public CustomService(CustomRepository repository)
        {
            _repository = repository;
        }
        public Task<Custom[]> GetCustoms()
        {
            return _repository.Select();
        }
        public Task<Custom> GetByDisanId(long disanId)
        {
            return _repository.First(c => c.DisanId == disanId);
        }
        public Task SetUser(User user)
        {
            return _repository.Where(c => c.User == user);
        }
    }
}
