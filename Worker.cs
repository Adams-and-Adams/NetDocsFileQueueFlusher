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
            _logger.LogInformation("File Queue Flusher V2 Started");

            ////var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesLIVE").Result; //  TODO : UNCOMMENT FOR LIVE
            //var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesDEV").Result; //  TODO : COMMENT FOR LIVE
            //if (!_dbConnectResult.IsSuccess)
            //{
            //    _logger.LogInformation($"{DateTime.Now} - {_dbConnectResult.Error}");
            //    return base.StopAsync(cancellationToken);
            //}
            //_sqlConnection = _dbConnectResult.Value;

            //var _settingsResult = new GetSettingsHelper(_sqlConnection).GetValues().Result;
            //if (!_settingsResult.IsSuccess)
            //{
            //    _logger.LogError(_dbConnectResult.Error);
            //    return base.StopAsync(cancellationToken);
            //}
            //_settingsModel = _settingsResult.Value;
            //_logger.LogInformation("Connected to DB");

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
            FileObject _fileObject;
            Paused = true;

            try
            {
                if (_sqlConnection == null) DoDbConnection();
                if (_sqlConnection.State == ConnectionState.Closed) DoDbConnection();
                if (_sqlConnection.State == ConnectionState.Broken)
                {
                    _sqlConnection.Close();
                    DoDbConnection();
                }

                var _getQueueResult = GetDataHelper.Helper(null, _sqlConnection, "AAMicroserviceLargeFileFlushQueue").Result;
                if (!_getQueueResult.IsSuccess)
                {
                    _logger.LogError($"Getting the File Queue - {_getQueueResult.Error}");
                    Paused = false;
                }
                else
                {
                    if (_getQueueResult.Value.Rows.Count > 0)
                    {
                        foreach (DataRow row in _getQueueResult.Value.AsEnumerable())
                        {
                            stopwatch.Restart();
                            _fileObject = new FileObject();

                            try
                            {
                                _fileObject.Guid = row.Field<string>("GUID");
                                _fileObject.SourceFile = row.Field<string>("SOURCEFILE");
                                _fileObject.NdFileName = row.Field<string>("DESTINATIONFILE");
                                _fileObject.NdUrl = row.Field<string>("NDURL");
                                _fileObject.NdFolderId = row.Field<string>("NDFOLDERID");

                                var _accessTokenResult = new AccessTokenHelper().GetAccessToken(_settingsModel.AccessTokenModel).Result;
                                if (!_accessTokenResult.IsSuccess)
                                {
                                    stopwatch.Stop();
                                    _logger.LogError($"ND Access Token Generated - {_accessTokenResult.Error}");
                                    _fileObject.Status = "Requested";
                                    UpdateQueue(_fileObject);
                                    Paused = false;
                                }
                                else
                                {
                                    var _accessToken = _accessTokenResult.Value;

                                    _logger.LogInformation($"ND Upload Started : {_fileObject.SourceFile}");
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

                                        _fileObject.Status = "Requested";
                                        UpdateQueue(_fileObject);
                                        Paused = false;
                                    }
                                    else
                                    {
                                        // Update the queue
                                        _fileObject.Status = "Completed";
                                        _fileObject.NdDocUrl = "https://eu.netdocuments.com/neWeb2/goId.aspx?id=" + _uploadResult.Value.id + "&open=Y";
                                        _fileObject.NdDocId = _uploadResult.Value.envId;
                                        _fileObject.Error = "";
                                        UpdateQueue(_fileObject);

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
                                        Paused = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"ND File Upload Failed : {ex.Message}");
                                _fileObject.Status = "Requested";
                                _fileObject.Error = $"Upload reset to Requested but an Error Occured : {ex.Message}";
                                UpdateQueue(_fileObject);
                                Paused = false;
                            }
                        }
                    }
                    else
                    {
                        Paused = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ND Upload Process Failed : {ex.Message}");
                Paused = false;
            }
        }

        private void UpdateQueue(FileObject _fileObject)
        {
            try
            {
                var _updateData = JsonConvert.SerializeObject(_fileObject);
                List<ProcParmObject> parmObj = new List<ProcParmObject>();
                parmObj.Add(new ProcParmObject { ParmName = "@x_data", ParmValue = _updateData });
                var _getUpdateQueueResult = GetDataHelper.Helper(parmObj, _sqlConnection, "AAMicroserviceLargeFileUpdateQueue").Result;
                if(!_getUpdateQueueResult.IsSuccess) _logger.LogError($"Update Queue Failed : {_getUpdateQueueResult.Error}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update Queue Failed : {ex.Message}");
                Paused = false;
            }
        }

        private void DoDbConnection()
        {
            var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesLIVE").Result; //  TODO : UNCOMMENT FOR LIVE
            //var _dbConnectResult = new DbConnectHelper(_configuration).OpenConnection("MicroservicesDEV").Result; //  TODO : COMMENT FOR LIVE
            if (!_dbConnectResult.IsSuccess) _logger.LogInformation($"{DateTime.Now} - {_dbConnectResult.Error}");
            _sqlConnection = _dbConnectResult.Value;

            var _settingsResult = new GetSettingsHelper(_sqlConnection).GetValues().Result;
            if (!_settingsResult.IsSuccess) _logger.LogError(_dbConnectResult.Error);
            
            _settingsModel = _settingsResult.Value;
            _logger.LogInformation("Connected to DB");
        }
    }
}