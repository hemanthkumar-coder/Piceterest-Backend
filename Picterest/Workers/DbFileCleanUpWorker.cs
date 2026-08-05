using Picterest.Services.Interface;

namespace Picterest.Workers
{
    public class DbFileCleanUpWorker : BackgroundService
    {
        private readonly ILogger<DbFileCleanUpWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbFileCleanUpWorker(ILogger<DbFileCleanUpWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Db File CleanUp Worker Start");

            using var scope = _scopeFactory.CreateScope();

            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var restorationTime = configuration["RestorationTime"] ?? throw new ArgumentException("Restoration Time is not Configured");

            if (!int.TryParse(restorationTime, out var delay))
            {
                _logger.LogError("Error Parsing Restoration Time");

                throw new InvalidOperationException("Error Parsing Restoration Time");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await fileService.DeleteStorageCleanedFilesFromDb();

                

                await Task.Delay(TimeSpan.FromDays(delay), stoppingToken);
            }

            _logger.LogInformation("Db File CleanUp Worker End");

        }
    }
}
