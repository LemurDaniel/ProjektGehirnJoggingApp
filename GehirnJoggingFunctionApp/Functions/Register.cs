using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Http;
using AzureFunctions;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;

namespace AzureFunctions
{
    public static class Register
    {
        [FunctionName("Register")]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Request<User> request = JsonConvert.DeserializeObject<Request<User>>(requestBody);
            User user = request.obj;
            string pass = request.TOKEN;

            // Generate TOKEN
            string AlphaNum = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random r = new Random();
            char[] token = new char[50];
            for (int i = 0; i < token.Length; i++) token[i] = AlphaNum[r.Next(0, AlphaNum.Length - 1)];

            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();
                    string sql = "Select id from Nutzer where nickname=@NAME";
                    using (SqlCommand com = new SqlCommand(sql, con))
                    {
                        com.Parameters.Add("@NAME", System.Data.SqlDbType.NVarChar);
                        com.Parameters["@NAME"].Value = user.name;

                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                request.Errorcode = -4;
                                throw new Exception("Nutzer Bereits vorhanden");
                            }
                        }
                    }

                    sql = "Insert Into Nutzer (nickname, pwd, blob) Values(@NAME, @PWD, @BLOB)";
                    using (SqlCommand com = new SqlCommand(sql, con))
                    {
                        com.Parameters.Add("@NAME", System.Data.SqlDbType.NVarChar);
                        com.Parameters.Add("@PWD", System.Data.SqlDbType.NVarChar);
                        com.Parameters.Add("@BLOB", System.Data.SqlDbType.NVarChar);

                        com.Parameters["@NAME"].Value = user.name;
                        com.Parameters["@PWD"].Value = BCrypt.Net.BCrypt.HashPassword(user.name + pass, BCrypt.Net.BCrypt.GenerateSalt());
                        com.Parameters["@BLOB"].Value = user.name.ToLower() + new string(token);

                        com.ExecuteNonQuery();
                    }

                    int id;
                    sql = "Select id from Nutzer where nickname=@NAME";
                    using (SqlCommand com = new SqlCommand(sql, con))
                    {
                        com.Parameters.Add("@NAME", System.Data.SqlDbType.NVarChar);
                        com.Parameters["@NAME"].Value = user.name;

                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            reader.Read();
                            id = reader.GetInt32(0);
                        }
                    }


                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
                    var client = storageAccount.CreateCloudBlobClient();
                    var cbContainer = client.GetContainerReference("images");
                    var container = client.GetContainerReference(user.name.ToLower() + new string(token));
                    await container.CreateIfNotExistsAsync();


                    //Blobs kopieren
                    ISet<String> blobs = await FUNCTIONS2.GetImages();
                    await FUNCTIONS2.CopyImages(cbContainer, container, blobs);
                    FUNCTIONS2.writeToDb(blobs, id, con);


                }
            }
            catch (SqlException e)
            {
                user.tempToken = null;
                request.Success = false;
                request.Error = e.Message;
                request.Errorcode = e.ErrorCode;
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(request))
                };
            }
            catch (Exception e)
            {
                user.tempToken = null;
                request.Success = false;
                request.Error = e.Message;
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(request))
                };
            }

            using (HttpClient http = new HttpClient())
            {
                return await http.PostAsync("https://projekt-gehirn-jogging-functions.azurewebsites.net/api/Login?code=Yued8GW/qIM7elGa/XcrSi4saQd5IQGS3atWGJ7VKh1Sz23X/jGT6Q==", new StringContent(JsonConvert.SerializeObject(request)));
            }
        }
    }

}

