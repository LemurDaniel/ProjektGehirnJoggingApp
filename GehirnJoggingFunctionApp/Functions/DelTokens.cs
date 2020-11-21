using System;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions
{
    public static class DelTokens
    {
        [FunctionName("DelTokens")]
        public static void Run([TimerTrigger("0 0 */12 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();
                    string sql = "Update Nutzer SET Token=null where DATEDIFF ( HOUR , tokenage , getDate() ) > 11";
                    using (SqlCommand com = new SqlCommand(sql, con))
                    {
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}
