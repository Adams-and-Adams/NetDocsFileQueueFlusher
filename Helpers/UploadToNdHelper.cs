using NetDocsFileQueueFlusher.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Data.SqlClient;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace NetDocsFileQueueFlusher.Helpers
{
    public class UploadToNdHelper
    {
        private readonly IRestHelper _restHelper;

        public UploadToNdHelper(IRestHelper restHelper)
        {
            _restHelper = restHelper;
        }

        public Result<UploadResultObject> UploadToND(SqlConnection _sqlConnection, FileObject _fileObject, string _accessToken)
        {
            string _guid = _fileObject.Guid;
            string _uploadFile = _fileObject.SourceFile;
            string _fileName = _fileObject.NdFileName;
            string _NdUrl = _fileObject.NdUrl;
            string _folderId = _fileObject.NdFolderId;

            var _convertResult = ConvertToBytes(_uploadFile).Result;

            if (!_convertResult.IsSuccess) return Result<UploadResultObject>.Failure(_convertResult.Error);
            var _fileContent = _convertResult.Value;

            RestRequest restRequest = _restHelper.FormRequest(Enums.RestType.Post, _NdUrl);
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddParameter("action", "upload");
            restRequest.AddFile("file", _fileContent, _fileName, "multipart/form-data");
            restRequest.AddParameter("destination", _folderId);
            restRequest.AddParameter("failOnError", "true");

            var restResponseResult = _restHelper.executeRequest(restRequest, _accessToken).Result;
            if (!restResponseResult.IsSuccess) return Result<UploadResultObject>.Failure(restResponseResult.Error);

            var _result = JsonConvert.DeserializeObject<UploadResultObject>(restResponseResult.Value.Content);

            return Result<UploadResultObject>.Success(_result);
        }

        private async Task<Result<byte[]>> ConvertToBytes(string _filename)
        {
            return await Task.Run(() =>
            {
                byte[] _bytes;

                try
                {
                    _bytes = System.IO.File.ReadAllBytes(_filename);
                }
                catch (Exception ex)
                {
                    return Result<byte[]>.Failure($"Error converting [{_filename}] to Byte Array : {ex.Message}");
                }
                
                return Result<byte[]>.Success(_bytes);
            });
        }
    }
}
