namespace Loader.API
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly CsvLoader _loader;

        public Worker(ILogger<Worker> logger, CsvLoader loader)
        {
            _logger = logger;
            _loader = loader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //_loader.LoadFiles(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}