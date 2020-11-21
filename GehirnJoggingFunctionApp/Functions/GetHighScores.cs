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
    public static class GetHighScores
    {

        [FunctionName("GetHighScores")]
        public static async System.Threading.Tasks.Task<HttpResponseMessage> runAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Request<SpielHighscores> request = JsonConvert.DeserializeObject<Request<SpielHighscores>>(requestBody);

            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();

                    // TOKEN überprüfen
                    HttpResponseMessage message = FUNCTIONS<SpielHighscores>.checkToken(request, con);
                    if (message != null) return message;

                    // Anfrage bearbeiten
                    request.obj = new SpielHighscores();

                    string sql = "Select * from Spiel";
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader reader = com1.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Spiel spiel = new Spiel();
                                request.obj.spiele.Add(reader.GetInt32(0), spiel);
                                spiel.id_spiel = reader.GetInt32(0);
                                spiel.spiel = reader.GetString(1);
                                spiel.globaleBestzeit = reader.GetInt32(2);
                                spiel.globalerDurschnitt = reader.GetInt32(3);
                                spiel.rekordhalter = reader.GetString(5);

                                if (spiel.globaleBestzeit == Int32.MaxValue)
                                {
                                    spiel.rekordhalter = "Niemand";
                                    spiel.globaleBestzeit = 0;
                                }
                            }
                        }

                        foreach (int key in request.obj.spiele.Keys)
                        {
                            sql = "Select * from Highscore where id_spiel=" + key + " and id_nutzer=" + request.Id;
                            using (SqlCommand com2 = new SqlCommand(sql, con))
                            {
                                using (SqlDataReader reader2 = com2.ExecuteReader())
                                {
                                    reader2.Read();
                                    Spiel spiel = request.obj.spiele[reader2.GetInt32(1)];
                                    spiel.bestzeit = reader2.GetInt32(2);
                                    spiel.schlechtesteZeit = reader2.GetInt32(3);
                                    spiel.durchschnitt = reader2.GetInt32(4);
                                    spiel.anzahl_gespielt = reader2.GetInt32(5);
                                    spiel.akummulierteSpielzeit = reader2.GetInt64(6);

                                    if (spiel.bestzeit == Int32.MaxValue) spiel.bestzeit = 0;
                                }
                            }
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
