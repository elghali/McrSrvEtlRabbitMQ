using AutoMapper;
using EventBus.Messages.Events;
using MassTransit;
using Parser.API.Entities;

namespace Parser.API.Parsers
{
    internal class CommonCsvParser : IParser
    {
        protected readonly ILogger<CommonCsvParser> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _outputColumns;
        private readonly string _outputTableName;
        private readonly string _loaderIncoming;
        private readonly string _parserProcessed;
        private readonly IMapper _mapper;
        private readonly IBusControl _busControl;

        public CommonCsvParser(ILogger<CommonCsvParser> logger, IConfiguration configuration, IMapper mapper, IBusControl busControl)
        {
            _logger = logger;
            _configuration = configuration;
            _outputColumns = configuration.GetValue<string>("ParserSettings:OutputColumns");
            _outputTableName = configuration.GetValue<string>("LoaderSettings:OutputTableName");
            _loaderIncoming = configuration.GetValue<string>("LoaderSettings:LoaderIncoming");
            _parserProcessed = configuration.GetValue<string>("ParserSettings:ParserProcessed");
            _mapper = mapper;
            _busControl = busControl;
        }

        public void ParseData(CancellationToken cts)
        {
            var incomingPath = _configuration.GetValue<string>("ParserSettings:ParserIncoming");
            var fileMask = _configuration.GetValue<string>("ParserSettings:ParserFilePattern");
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

                foreach (FileInfo file in files)
                {
                    string outputFileName = string.Empty;
                    try
                    {
                        outputFileName = TransformData(file);
                        //Generate event
                        ParserFile parserFile = new ParserFile();
                        parserFile.OutputFileName = outputFileName;
                        parserFile.OutputTableName = _outputTableName;
                        parserFile.IncomingPath = _loaderIncoming;
                        _ = ParserFile(parserFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("File: " + file.FullName + "Error: " + ex.Message + " InnerException: " + ex.InnerException);
                        _logger.LogWarning("Skipping to next file...");
                    }
                }
            }
        }
        private async Task ParserFile(ParserFile parserFile)
        {
            var eventMessage = _mapper.Map<ParserFileEvent>(parserFile);
            try
            {
                await _busControl.Publish(eventMessage);
                _logger.LogDebug("Parser sending message at: {time}", DateTimeOffset.UtcNow);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string TransformData(FileInfo file)
        {
            string[] outputColumns = _outputColumns.Split(',');
            List<string> finaleLines = new List<string>();
            string outputFileName = string.Empty;

            try
            {
                using (var reader = new StreamReader(file.FullName))
                {
                    string[]? header = default;
                    string? line = reader.ReadLine();
                    List<int> validIndices = new List<int>();

                    if (line != null)
                        header = line.Split(',');

                    if (header != null)
                    {
                        //Keep only wanted columns
                        validIndices = GetIndices(header.ToList());
                    }

                    //Add Header first
                    finaleLines.Add(_outputColumns);

                    //Add rest of lines
                    while ((line = reader.ReadLine()) != null)
                    {
                        var finalLine = line.Split(',').Where((x, index) => validIndices.Contains(index))
                            .ToList().Aggregate((x, y) => x + "," + y);

                        finaleLines.Add(finalLine);
                    }
                }
                outputFileName = WriteToFile(file.Name, finaleLines);
                MoveProcessedFile(file);
                return outputFileName;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GetOutputFileName(string fileName)
        {
            return _outputTableName + "_" +
                Path.GetFileNameWithoutExtension(fileName) + "_" + GetDateFormatted() + ".csv";
        }

        private string GetDateFormatted()
        {
            return DateTime.Now.ToString("yyyyMMddhhmmssmmm");
        }

        private List<int> GetIndices(List<string> header)
        {
            List<int> indices = new List<int>();
            foreach (string column in _outputColumns.Split(','))
            {
                indices.Add(header.IndexOf(column));
            }
            return indices;
        }
        private string WriteToFile(string fileName, List<string> finaleLines)
        {
            string outputFileName = GetOutputFileName(fileName);
            File.WriteAllLines(Path.Combine(_loaderIncoming, outputFileName), finaleLines);
            return outputFileName;
        }
        private void MoveProcessedFile(FileInfo file)
        {
            if (file.Exists)
                File.Move(file.FullName, Path.Combine(_parserProcessed, file.Name));
        }
    }
}
