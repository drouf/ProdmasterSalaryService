using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.ViewModels.Account;

namespace ProdmasterSalaryService.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> Add(string login, string password);
        Task<User> Add(RegisterModel model, Custom custom);
        Task<User> Get(string login, string password);
        Task<User> GetByDisanId(long disanId);
        Task<User> GetByLogin(string? login);
        UserModel? GetUserModelByUser(User? user);
        Task<User> UpdateUser(User user);
        Task<bool> UserExists(RegisterModel model);
    }
}
