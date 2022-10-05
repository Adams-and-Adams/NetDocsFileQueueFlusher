using NetDocsFileQueueFlusher.Helpers;
using NetDocsFileQueueFlusher.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

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
        public Stopwatch? stopwatch;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IRestHelper restHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _restHelper = restHelper;

            Paused = false;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("");
            _logger.LogInformation("=========================================================================================");
            _logger.LogInformation("File Queue Flusher Started");

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
                _logger.LogError(_dbConnectResult.Error);
                return base.StopAsync(cancellationToken);
            }
            _settingsModel = _settingsResult.Value;
            _logger.LogInformation("Connected to DB");

            stopwatch = new Stopwatch();

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("File Queue Flusher Stopped");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(!Paused) DoWork();
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void DoWork()
        {
            Paused = true;

            try
            {
                var _getQueueResult = GetDataHelper.Helper(null, _sqlConnection, "AAMicroserviceLargeFileFlushQueue").Result;
                if (!_getQueueResult.IsSuccess)
                {
                    _logger.LogInformation($"Getting the File Queue - {_getQueueResult.Error}");
                }
                else
                {

                    if (_getQueueResult.Value.Rows.Count > 0)
                    {
                        foreach (DataRow row in _getQueueResult.Value.AsEnumerable())
                        {
                            stopwatch.Restart();

                            var _accessTokenResult = new AccessTokenHelper().GetAccessToken(_settingsModel.AccessTokenModel).Result;
                            if (!_accessTokenResult.IsSuccess)
                            {
                                stopwatch.Stop();
                                _logger.LogError($"ND Access Token Generated - {_accessTokenResult.Error}");
                            }
                            else
                            {
                                FileObject _fileObject = new FileObject();
                                _fileObject.Guid = row.Field<string>("GUID");
                                _fileObject.SourceFile = row.Field<string>("SOURCEFILE");
                                _fileObject.NdFileName = row.Field<string>("DESTINATIONFILE");
                                _fileObject.NdUrl = row.Field<string>("NDURL");
                                _fileObject.NdFolderId = row.Field<string>("NDFOLDERID");

                                _logger.LogInformation($"ND Upload Started : {_fileObject.SourceFile}");

                                var _accessToken = _accessTokenResult.Value;
                                _logger.LogInformation($"ND Access Token Generated : Successful");

                                var _uploadResult = new UploadToNdHelper(_restHelper).UploadToND(_sqlConnection, _fileObject, _accessToken);
                                if (!_uploadResult.IsSuccess)
                                {
                                    stopwatch.Stop();
                                    TimeSpan timeTaken = stopwatch.Elapsed;
                                    string _elapse = timeTaken.ToString(@"m\:ss\.fff");
                                    _fileObject.Status = "Failed";
                                    _fileObject.Error = _uploadResult.Error;
                                    _logger.LogError($"ND Upload Failed ({_elapse}) - Filename [{_fileObject.NdFileName}], Source [{Path.GetFileName(_fileObject.SourceFile)}] : {_uploadResult.Error}");
                                }
                                else
                                {
                                    // Update the queue
                                    _fileObject.Status = "Completed";
                                    _fileObject.NdDocUrl = "https://eu.netdocuments.com/neWeb2/goId.aspx?id=" + _uploadResult.Value.id + "&open=Y";
                                    _fileObject.NdDocId = _uploadResult.Value.envId;
                                    var _updateData = JsonConvert.SerializeObject(_fileObject);
                                    List<ProcParmObject> parmObj = new List<ProcParmObject>();
                                    parmObj.Add(new ProcParmObject { ParmName = "@x_data", ParmValue = _updateData });
                                    var _getUpdateQueueResult = GetDataHelper.Helper(parmObj, _sqlConnection, "AAMicroserviceLargeFileUpdateQueue").Result;

                                    // Update the Logger
                                    stopwatch.Stop();
                                    TimeSpan timeTaken = stopwatch.Elapsed;
                                    string _elapse = timeTaken.ToString(@"m\:ss\.fff");

                                    if (!_getQueueResult.IsSuccess)
                                        _logger.LogError($"Update Queue - {_getQueueResult.Error}");
                                    else
                                    {
                                        _logger.LogInformation($"ND Upload Completed ({_elapse}) - Filename [{_fileObject.NdFileName}], Source [{Path.GetFileName(_fileObject.SourceFile)}]");
                                        File.Delete(_fileObject.SourceFile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ND Upload Process Failed : {ex.Message}");
            }

            Paused = false;
        }
    }
}