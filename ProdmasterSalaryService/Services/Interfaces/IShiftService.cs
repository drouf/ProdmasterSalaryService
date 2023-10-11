using ProdmasterSalaryService.Models.Classes;
using System.Runtime.ExceptionServices;

namespace ProdmasterSalaryService.Services.Interfaces
{
    public interface IShiftService
    {
        Task<Shift> FirstByDateAsync(DateTime date, User user);
        Task<bool> AddRemoteShift(DateTime date, User user);
        Task<bool> RemoveRemoteShift(DateTime date, User user);
    }
}
