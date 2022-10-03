using NetDocsFileQueueFlusher.Helpers;
using NetDocsFileQueueFlusher.Models;
using System.Data;
using System.Data.SqlClient;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace NetDocsFileQueueFlusher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRestHelper _restHelper;

        public bool Paused { get; set; }
        public SqlConnection _sqlConnection = new SqlConnection();
        public SettingsModel _settingsModel = new SettingsModel();

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IRestHelper restHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _restHelper = restHelper;

            Paused = false;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Worker Started at: {time}", DateTimeOffset.Now);

            //var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesLIVE").Result; // UNCOMMENT FOR LIVE
            var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesDEV").Result; // COMMENT FOR LIVE
            if (!_dbConnectResult.IsSuccess)
            {
                _logger.LogInformation($"{DateTime.Now} - {_dbConnectResult.Error}");
                return base.StopAsync(cancellationToken);
            }
            _sqlConnection = _dbConnectResult.Value;

            var _settingsResult = new GetSettingsHelper(_sqlConnection).GetValues().Result;
            if (!_settingsResult.IsSuccess)
            {
                _logger.LogInformation($"{DateTime.Now} - {_dbConnectResult.Error}");
                return base.StopAsync(cancellationToken);
            }
            _settingsModel = _settingsResult.Value;

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(!Paused)
                {
                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    DoWork();
                }
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async void DoWork()
        {
            Paused = true;

            try
            {
                var _getQueueResult = await GetDataHelper.Helper(null, _sqlConnection, "AAMicroserviceLargeFileFlushQueue");
                if (!_getQueueResult.IsSuccess)
                {
                    _logger.LogInformation($"{DateTime.Now} - {_getQueueResult.Error}");
                }
                else
                {
                    if (_getQueueResult.Value.Rows.Count > 0)
                    {
                        var _accessTokenResult = await new AccessTokenHelper().GetAccessToken(_settingsModel.AccessTokenModel);
                        if (!_accessTokenResult.IsSuccess)
                        {
                            _logger.LogInformation($"{DateTime.Now} - {_accessTokenResult.Error}");
                        }
                        var _accessToken = _accessTokenResult.Value;

                        List<FileObject> _updateQueue = new List<FileObject>();
                        foreach (DataRow row in _getQueueResult.Value.AsEnumerable())
                        {
                            FileObject _fileObject = new FileObject();
                            _fileObject.Guid = row.Field<string>("GUID");
                            _fileObject.SourceFile = row.Field<string>("SOURCEFILE");
                            _fileObject.NdUrl = row.Field<string>("NDURL");
                            _fileObject.FolderId = row.Field<string>("FOLDERID");

                            var _uploadResult = await new UploadToNdHelper(_restHelper).UploadToND(_sqlConnection, _fileObject, _accessToken);
                            if (!_uploadResult.IsSuccess)
                            {
                                _logger.LogInformation($"{DateTime.Now} - {_uploadResult.Error}");
                            }
                            else
                            {
                                var _getUpdateQueueResult = await GetDataHelper.Helper(null, _sqlConnection, "AAMicroserviceLargeFileFlushQueue");
                                if (!_getQueueResult.IsSuccess)
                                    _logger.LogInformation($"{DateTime.Now} - {_getQueueResult.Error}");
                                else
                                    _updateQueue.Add(_fileObject);
                            }
                        }

                        foreach (var _item in _updateQueue)
                        {
                            // Update the queue
                            // Remove the file
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{DateTime.Now} - Upload Process Faile : {ex.Message}");
            }

            Paused = false;
        }  
    }
}