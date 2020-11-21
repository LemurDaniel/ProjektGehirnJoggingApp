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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFunctions.Functions
{
    public static class GetStandardImages
    {
        [FunctionName("GetStandardImages")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
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


                    ISet<String> blobs = await FUNCTIONS2.GetImages();
                    string blob = null;

                    string sql = "Select blob from Nutzer where id=" + request.Id;
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader reader = com1.ExecuteReader())
                        {
                            while (reader.Read()) blob = reader.GetString(0);
                        }
                    }

                    sql = "Select * from imageNutzer where id_nutzer=" + request.Id;
                    using (SqlCommand com1 = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader reader = com1.ExecuteReader())
                        {
                            while (reader.Read()) if (blobs.Contains(reader.GetString(1))) blobs.Remove(reader.GetString(1));
                        }
                    }

                    if (blobs.Count == 0) {
						request.Errorcode = -11;
						throw new Exception("");
					}

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
                    var client = storageAccount.CreateCloudBlobClient();
                    var cbContainer = client.GetContainerReference("images");
                    var container = client.GetContainerReference(blob);

                    //Blobs kopieren
                    await FUNCTIONS2.CopyImages(cbContainer, container, blobs);
                    FUNCTIONS2.writeToDb(blobs, request.Id, con);
                    request.obj = blobs;

                }
                request.Success = true;
            }
            catch (SqlException e)
            {
                request.Success = false;
                request.Error = e.Message;
                request.Errorcode = e.ErrorCode;
            }
			catch (Exception e1)
            {
                request.Success = false;
                request.Error = "Ein Fehler ist aufgetreten";
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(request))
            };
        }
    }
}
