using NetDocsFileQueueFlusher.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class GetSettingsHelper
    {
        SqlConnection _sqlConnection { get; set; }

        public GetSettingsHelper(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public async Task<Result<SettingsModel>> GetValues()
        {
            var _readSettingsResult = await ReadSettings(_sqlConnection);
            if (!_readSettingsResult.IsSuccess) return Result<SettingsModel>.Failure(_readSettingsResult.Error);
            var _readSettings = _readSettingsResult.Value;

            SettingsModel _settings = new SettingsModel();
            foreach (DataRow row in _readSettings.AsEnumerable())
            {
                string area = row.Field<string>("o_Area");

                if (area == "Globals")
                {
                    switch (row.Field<string>("o_Setting"))
                    {
                        case "DocumentServerFolder":
                            _settings.DocumentServerFolder = row.Field<string>("o_Value");
                            break;
                        case "DocumentMapDrive":
                            _settings.DocumentMapDrive = row.Field<string>("o_Value");
                            break;
                    }
                }
                if (area == "AccessToken")
                {
                    switch (row.Field<string>("o_Setting"))
                    {
                        case "TokenUrl":
                            _settings.AccessTokenModel.AccessTokenUrl = row.Field<string>("o_Value");
                            break;
                        case "RepositoryId":
                            _settings.AccessTokenModel.AccessTokenRespositoryId = row.Field<string>("o_Value");
                            break;
                        case "ClientId":
                            _settings.AccessTokenModel.AccessTokenClientId = row.Field<string>("o_Value");
                            break;
                        case "SecretCode":
                            _settings.AccessTokenModel.AccessTokenClientSecret = row.Field<string>("o_Value");
                            break;
                    }
                }
            }
            return Result<SettingsModel>.Success(_settings);
        }

        private async Task<Result<DataTable>> ReadSettings(SqlConnection conn)
        {
            DataTable td = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetGlobalSettings";
                cmd.Parameters.AddWithValue("@x_environment", "NetDocs");

                await Task.Run(() => da.Fill(td));
                return Result<DataTable>.Success(td);
            }
            catch (Exception ex)
            {
                return Result<DataTable>.Failure(ex.Message);
            }
        }
    }
}
