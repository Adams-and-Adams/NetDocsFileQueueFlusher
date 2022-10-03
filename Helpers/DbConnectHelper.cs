using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class DbConnectHelper
    {

        private readonly IConfiguration _configuration;

        public DbConnectHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CloseConnection(SqlConnection sqlConnection)
        {
            if (sqlConnection != null)
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        public async Task<Result<SqlConnection>> OpenConnection(string connectTo)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString(connectTo);
                var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                return Result<SqlConnection>.Success(conn);
            }
            catch (Exception ex)
            {
                return Result<SqlConnection>.Failure($"Failed to Connect to Database : {ex.Message}");
            }
        }
    }
}

