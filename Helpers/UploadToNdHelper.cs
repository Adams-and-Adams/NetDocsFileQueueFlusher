using NetDocsFileQueueFlusher.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class UploadToNdHelper
    {
        private readonly IRestHelper _restHelper;

        public UploadToNdHelper(IRestHelper restHelper)
        {
            _restHelper = restHelper;
        }

        public async Task<Result<string>> UploadToND(SqlConnection _sqlConnection, FileObject _fileObject, string _accessToken)
        {
            string _guid = _fileObject.Guid;
            string _uploadFile = _fileObject.SourceFile;
            string _fileName = Path.GetFileName(_uploadFile); 
            string _NdUrl = _fileObject.NdUrl;
            string _folderId = _fileObject.FolderId;

            var _convertResult = await ConvertToBytes(_uploadFile);
            if (!_convertResult.IsSuccess) return Result<string>.Failure(_convertResult.Error);
            var _fileContent = _convertResult.Value;

            RestRequest restRequest = _restHelper.FormRequest(Enums.RestType.Post, _NdUrl);
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddParameter("action", "upload");
            restRequest.AddFile("file", _fileContent, _fileName, "multipart/form-data");
            restRequest.AddParameter("destination", _folderId);
            restRequest.AddParameter("failOnError", "true");

            var restResponseResult = await _restHelper.executeRequest(restRequest, _accessToken);
            if (!restResponseResult.IsSuccess) return Result<string>.Failure(restResponseResult.Error);

            return Result<string>.Success("Successful");
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
