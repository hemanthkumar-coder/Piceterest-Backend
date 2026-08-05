using Picterest.Services.Interface;

namespace Picterest.Workers
{
    public class StorageCleanUpWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<StorageCleanUpWorker> _logger;
        
        public StorageCleanUpWorker(IServiceScopeFactory serviceScopeFactory, ILogger<StorageCleanUpWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Storage Clean Up Worker Execution Start");


            using var scope = _serviceScopeFactory.CreateScope();

            var cleanUpService = scope.ServiceProvider.GetRequiredService<ICleanUpStorageService>();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var restorationTime = configuration["RestorationTime"] ?? throw new ArgumentNullException("Restoration Time is not Provided");

            if (!int.TryParse(restorationTime, out var delay))
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogError("Cannot Parse Restoration Time");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                
                await cleanUpService.ProcessImageStorageCleanUp();

                

                await Task.Delay(TimeSpan.FromDays(delay), stoppingToken);
            }

            _logger.LogInformation("Storage Clean Up Worker Execution End");

        }
    }
}
