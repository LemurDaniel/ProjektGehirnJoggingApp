using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Text;
using static AzureFunctions.SpielHighscores;

namespace AzureFunctions
{
    public static class UpdateHighScores
    {

        [FunctionName("UpdateHighscore")]
        public static HttpResponseMessage run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Request<Spiel> request = JsonConvert.DeserializeObject<Request<Spiel>>(requestBody);

            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();
                    // TOKEN überprüfen
                    HttpResponseMessage message = FUNCTIONS<Spiel>.checkToken(request, con);
                    if (message != null) return message;


                    // Anfrage bearbeiten
                    string sql = "Update Highscore Set bestzeit=@BEST, schlechteste_Zeit=@BAD, durchschnitt=@AVERG, anzahl=@COU, kumulierteSpielzeit=@KUM Where id_Spiel=@SID and id_Nutzer=@NID";
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        com1.Parameters.Add("@BEST", System.Data.SqlDbType.Int);
                        com1.Parameters.Add("@BAD", System.Data.SqlDbType.Int);
                        com1.Parameters.Add("@AVERG", System.Data.SqlDbType.Int);
                        com1.Parameters.Add("@COU", System.Data.SqlDbType.Int);
                        com1.Parameters.Add("@KUM", System.Data.SqlDbType.BigInt);

                        com1.Parameters.Add("@SID", System.Data.SqlDbType.Int);
                        com1.Parameters.Add("@NID", System.Data.SqlDbType.Int);


                        com1.Parameters["@BEST"].Value = request.obj.bestzeit;
                        com1.Parameters["@BAD"].Value = request.obj.schlechtesteZeit;
                        com1.Parameters["@AVERG"].Value = request.obj.durchschnitt;
                        com1.Parameters["@COU"].Value = request.obj.anzahl_gespielt;
                        com1.Parameters["@KUM"].Value = request.obj.akummulierteSpielzeit;

                        com1.Parameters["@SID"].Value = request.obj.id_spiel;
                        com1.Parameters["@NID"].Value = request.Id;

                        com1.ExecuteNonQuery();
                    }


                    sql = "Select * from Spiel where id=" + request.obj.id_spiel;
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader reader = com1.ExecuteReader())
                        {
                            reader.Read();
                            request.obj.globaleBestzeit = reader.GetInt32(2);
                            request.obj.globalerDurschnitt = reader.GetInt32(3);
                            request.obj.rekordhalter = reader.GetString(5);
                        }
                    }
                }
                request.Success = true;
            }
            catch (SqlException e)
            {
                request.Success = false;
                request.Error = e.Message;
                request.Errorcode = e.ErrorCode;
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(request))
            };
        }
    }
}
