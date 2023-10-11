using Newtonsoft.Json;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProdmasterSalaryService.Services.Classes
{
    public class UpdateOperationsService : IUpdateOperationsService
    {
        private readonly HttpClient _httpClient;
        private readonly OperationRepository _operationRepository;
        private readonly CustomRepository _customRepository;
        private const string url = "http://192.168.1.251:8444/api";
        public UpdateOperationsService(HttpClient httpClient, OperationRepository repository, CustomRepository customRepository)
        {
            _httpClient = httpClient;
            _operationRepository = repository;
            _customRepository = customRepository;
        }

        public async Task Update()
        {
            var operations = await GetOperations();
            if (operations != null)
            {
                await UpdateOperations(operations);
            }
        }

        private async Task UpdateOperations(IEnumerable<Operation> operations)
        {
            if (operations.Any())
            {
                var propertiesToUpdate = new List<string>()
                {
                    nameof(Operation.Sum),
                    nameof(Operation.Saled),
                    nameof(Operation.Paid),
                    nameof(Operation.Note),
                    nameof(Operation.Date),
                    nameof(Operation.Object),
                    nameof(Operation.Custom)
                };
                foreach (var operation in operations)
                {
                    var custom = await _customRepository.First(c => c.DisanId == operation.Object);
                    if (custom != null)
                    {
                        operation.Custom = custom;
                    }
                    operation.Date = DateTime.UtcNow;
                }
                operations = operations.Where(c => c.DisanId != 0);
                await _operationRepository.AddOrUpdateRange(operations, propertiesToUpdate);
            }
        }

        private async Task<IEnumerable<Operation>?> GetOperations()
        {
            var query = "\"select j.number, j.sum, j.saled, j.paid, j.rem1, j.object " +
                "from journal.dbf as j " +
                "inner join custom.dbf as c on j.object == c.number and c.parent == 1971601 " +
                "where not(j.arch) and j.opera == 6\"";
            return await GetObjectsFromQueryAsync<IEnumerable<Operation>>(query);
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
