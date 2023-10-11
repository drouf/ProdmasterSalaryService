using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;
using ProdmasterSalaryService.ViewModels.Account;
using System.Diagnostics.Eventing.Reader;

namespace ProdmasterSalaryService.Services.Classes
{
    public class UserService : IUserService
    {
        private readonly UserRepository _repository;
        private readonly CustomRepository _customRepository;
        public UserService(UserRepository repository, CustomRepository customRepository)
        {
            _repository = repository;
            _customRepository = customRepository;
        }
        public Task<User> Add(string login, string password)
        {
            return _repository.Add(new User() { Login = login, Password = password });
        }

        public async Task<User> Add(RegisterModel model, Custom custom)
        {
            var user = new User()
            {
                DisanId = model.DisanId,
                Login = model.Login,
                Password = model.Password,
                Custom = custom,
                CustomId = custom.Id
            };
            custom.User = user;
            var response = await _repository.Add(user);

            await _customRepository.Update(custom);

            return response;
        }

        public Task<User> Get(string login, string password)
        {
            return _repository.First(c => c.Login == login && c.Password == password);
        }

        public Task<User> GetByDisanId(long disanId)
        {
            return _repository.First(c => c.DisanId == disanId);
        }
        public Task<User> GetByLogin(string login)
        {
            return _repository.First(c => c.Login == login);
        }

        public async Task<UserModel> GetModelFromUser(User user)
        {
            return new UserModel
            {
                DisanId = user.DisanId,
                Name = user.Custom.Name,
                Login = user.Login,
                Salary = user.Custom.Salary,
            };
        }

        public Task<User> UpdateUser(User user)
        {
            return _repository.Update(user);
        }

        public async Task<bool> UserExists(RegisterModel model)
        {
            var user = await _repository.First(c => c.Login == model.Login || c.DisanId == model.DisanId);
            return !ReferenceEquals(user, null);
        }
    }
}
