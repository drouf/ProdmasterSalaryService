using ProdmasterSalaryService.Services.Interfaces;

namespace ProdmasterSalaryService.Services.Hosted
{
    public class UpdateOperationsHostedService : IHostedService
    {
        private readonly ILogger<UpdateOperationsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public UpdateOperationsHostedService(ILogger<UpdateOperationsHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Не блокируем поток выполнения: StartAsync должен запустить выполнение фоновой задачи и завершить работу
            UpdateOperations(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task UpdateOperations(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var updateService = scope.ServiceProvider.GetRequiredService<IUpdateOperationsService>();
                    await updateService.Update();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to update operations\nError: {ex}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Если нужно дождаться завершения очистки, но контролировать время, то стоит предусмотреть в контракте использование CancellationToken
            //await someService.DoSomeCleanupAsync(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
