using Newtonsoft.Json;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace ProdmasterSalaryService.Services.Classes
{
    public class UpdateCustomsService : IUpdateCustomsService
    {
        private readonly HttpClient _httpClient;
        private readonly UserRepository _userRepository;
        private readonly CustomRepository _customRepository;
        private const string url = "http://192.168.1.251:8444/api";
        public UpdateCustomsService(HttpClient httpClient, CustomRepository repository, UserRepository userRepository)
        {
            _httpClient = httpClient;
            _customRepository = repository;
            _userRepository = userRepository;
        }
        public async Task Update() 
        {
            var customs = await GetCustoms();
            if (customs != null)
            {
                await UpdateCustoms(customs);
                await SynchronizeUC();
            }
        }

        private async Task SynchronizeUC()
        {
            var users = await _userRepository.Select();
            foreach (var user in users)
            {
                var custom = user.Custom ?? await _customRepository.First(c => c.DisanId == user.DisanId);
                if (custom != null)
                {
                    custom.User = user;
                    await _customRepository.Update(custom);
                }
            }
        }
        
        private Task UpdateCustoms(IEnumerable<Custom> customs)
        {
            if (customs.Any())
            {
                var propertiesToUpdate = new List<string>()
                {
                    nameof(Custom.Salary),
                    nameof(Custom.Name),
                };
                customs = customs.Where(c =>  c.DisanId != 0);
                foreach (var custom in customs)
                {
                    custom.Name = custom.Name.Trim();
                }
                return _customRepository.AddOrUpdateRange(customs, propertiesToUpdate);
            }
            return Task.CompletedTask;
        }

        private async Task<IEnumerable<Custom>?> GetCustoms()
        {
            var query = "\"select cus.number, cus.name, IIF(ISNULL(VAL(cusa.cvalue)), 0, INT(VAL(cusa.cvalue))) as salary " +
                "from custom.dbf as cus " +
                "left join customattribute.dbf as cusa on cusa.r1 == cus.number and alltrim(cusa.name) == alltrim(upper('оклад')) " +
                "where not('Стажёр'$cus.crem) and parent==1971601\"";
            return await GetObjectsFromQueryAsync<IEnumerable<Custom>>(query);
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
