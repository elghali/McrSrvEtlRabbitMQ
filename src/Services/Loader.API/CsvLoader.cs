using AutoMapper;
using EventBus.Messages.Events;
using Loader.API.Entities;
using MassTransit;

namespace Loader.API
{
    public class CsvLoader : BackgroundService, IConsumer<ParserFileEvent>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsvLoader> _logger;
        private readonly IMapper _mapper;

        public CsvLoader(IConfiguration configuration, ILogger<CsvLoader> logger, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public Task Consume(ConsumeContext<ParserFileEvent> context)
        {
            var command = _mapper.Map<LoaderFileCommand>(context.Message);
            var fileMask = command.OutputTableName;
            var incomingPath = command.IncomingPath;
            _logger.LogDebug("Loader Receiving message at: {time}", DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        public void LoadFiles(CancellationToken cts)
        {
            var incomingPath = _configuration.GetValue<string>("LoaderSettings:LoaderIncoming");
            var fileMask = _configuration.GetValue<string>("LoaderSettings:OutputTableName");

            if (!Directory.Exists(incomingPath))
            {
                _logger.LogError("Source Directory does not exist");
                throw new IOException("Directory does not exist!");
            }
            while (!cts.IsCancellationRequested)
            {
                IEnumerable<FileInfo> files = new DirectoryInfo(incomingPath)
                    .EnumerateFiles(fileMask)
                    .OrderByDescending(x => x.LastWriteTime)
                    .Take(10);

                _logger.LogInformation("Loading Data for files: " + String.Join(',', files.Select(x => x.Name).ToArray()));
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //_loader.LoadFiles(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
