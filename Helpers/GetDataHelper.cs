using NetDocsFileQueueFlusher.Models;
using System.Data;
using System.Data.SqlClient;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class GetDataHelper
    {
        public static async Task<Result<DataTable>> Helper(List<ProcParmObject> parmObj, SqlConnection conn, string proc)
        {
            DataTable td = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = proc;

                if (parmObj != null)
                {
                    foreach (var item in parmObj)
                    {
                        cmd.Parameters.AddWithValue(item.ParmName, item.ParmValue);
                    }
                }
                await Task.Run(() => da.Fill(td));
            }
            catch (Exception ex)
            {
                Result<DataTable>.Failure($"Failed to execute Procedure {proc} : {ex.Message}");
            }

            return Result<DataTable>.Success(td);
        }
    }
}
