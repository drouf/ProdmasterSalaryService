using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;

namespace ProdmasterSalaryService.Services.Classes
{
    public class ShiftService : IShiftService
    {
        private readonly ShiftRepository _repository;

        public ShiftService(ShiftRepository repository)
        {
            _repository = repository;
        }
        public async Task<Shift> FirstByDateAsync(DateTime date, User user)
        {
            return await _repository.First(s => s.Start!=null && s.Start.Value.Date == date.Date && s.Custom.Id == user.Custom.Id);
        }

        public async Task<bool> AddRemoteShift(DateTime date, User user)
        {
            var custom = user.Custom;
            var shift = new Shift()
            {
                DisanId = await GenerateFakeDisanId(),
                Start = date,
                End = date,
                Coefficient = 0.5,
                Object = (custom != null) ? custom.DisanId : 0,
                Custom = custom,
                
            };
            var addedShift = await _repository.Add(shift);
            if (addedShift != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RemoveRemoteShift(DateTime date, User user)
        {
            var shift = await _repository.First(s => s.Start != null && s.Start.Value.Date == date.Date && s.Custom.Id == user.Custom.Id);
            if(shift != null)
            {
                await _repository.Remove(shift);
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<long> GenerateFakeDisanId()
        {
            var minDisanId = (await _repository.Select()).Min(d => d.DisanId);
            if(minDisanId >= 0)
            {
                return -101;
            }
            else
            {
                return minDisanId - 100;
            }
        }
    }
}
