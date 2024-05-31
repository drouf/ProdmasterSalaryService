using Newtonsoft.Json;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace ProdmasterSalaryService.Services.Classes
{
    public class UpdateShiftsService : IUpdateShiftsService
    {
        private readonly HttpClient _httpClient;
        private readonly ShiftRepository _shiftRepository;
        private readonly CustomRepository _customRepository;
        private const string url = "http://192.168.1.251:8444/api";
        public UpdateShiftsService(HttpClient httpClient, ShiftRepository repository, CustomRepository customRepository)
        {
            _httpClient = httpClient;
            _shiftRepository = repository;
            _customRepository = customRepository;
        }

        public async Task Update()
        {
            var shifts = await GetShifts();
            if (shifts != null)
            {
                await UpdateShifts(shifts);
            }
        }

        private async Task UpdateShifts(IEnumerable<Shift> shifts)
        {
            if (shifts.Any())
            {
                var propertiesToUpdate = new List<string>()
                {
                    nameof(Shift.Start),
                    nameof(Shift.End),
                    nameof(Shift.Object),
                    nameof(Shift.Custom),
                    nameof(Shift.Coefficient)
                };
                foreach (var shift in shifts)
                {
                    var custom = await _customRepository.First(c => c.DisanId == shift.Object);
                    if (custom != null)
                    {
                        shift.Custom = custom;
                    }
                }
                //shifts = shifts.Where(c => c.DisanId != 0);
                await _shiftRepository.AddOrUpdateRange(shifts, propertiesToUpdate);
            }
        }

        private async Task<IEnumerable<Shift>?> GetShifts()
        {
            var shifts = await GetShiftsFromTable("worktime");
            var archShifts = await GetShiftsFromTable("worktimearch");
            var arch2020Shifts = await GetShiftsFromTable("worktimearch20201231");
            if (shifts != null && archShifts != null && archShifts.Any())
            {
                shifts = shifts.Union(archShifts);
                if(arch2020Shifts != null)
                {
                    shifts = shifts.Union(arch2020Shifts);
                }
            }
            return shifts;
        }

        private async Task<IEnumerable<Shift>?> GetShiftsFromTable(string tableName)
        {
            var query = "\"select wt.number, wt.idn as object, wt.timebeg, wt.timeend, 1 as coefficient " +
                $"from {tableName} as wt " +
                "inner join custom.dbf as c on wt.idn == c.number and c.parent == 1971601\"";
            return await GetObjectsFromQueryAsync<IEnumerable<Shift>>(query);
        }

        private async Task<T?> GetObjectsFromQueryAsync<T>(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var content = new StringContent(query, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                var result = await _httpClient.PostAsync(url, content);
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var dataString = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(dataString))
                    {
                        return GetObjectFromDataString<T>(dataString);
                    }
                }
            }
            return default;
        }

        private T? GetObjectFromDataString<T>(string dataString)
        {
            return JsonConvert.DeserializeObject<T>(dataString);
        }
    }
}
