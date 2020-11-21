using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;

namespace AzureFunctions.Functions
{
    public static class GetImageFiles
    {
        [FunctionName("GetImageFiles")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Request<ISet<String>> request = JsonConvert.DeserializeObject<Request<ISet<String>>>(requestBody);

            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();
                    // TOKEN überprüfen
                    HttpResponseMessage message = FUNCTIONS<ISet<String>>.checkToken(request, con);
                    if (message != null) return message;


                    // Anfrage bearbeiten
                    request.obj = new HashSet<String>();
                    string sql = "Select * from imageNutzer where id_nutzer=" + request.Id;
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader reader = com1.ExecuteReader())
                        {
                            while (reader.Read()) request.obj.Add(reader.GetString(1));
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
