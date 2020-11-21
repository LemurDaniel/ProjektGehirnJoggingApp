using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;

namespace AzureFunctions
{
    public static class Function2
    {
        [FunctionName("Login")]
        public static async System.Threading.Tasks.Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Request<User> request = JsonConvert.DeserializeObject<Request<User>>(requestBody);
            User user = request.obj;
            string pass = request.TOKEN;
            request.TOKEN = null;

            try
            {
                using (SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    con.Open();
                    string pwdhash;

                    // Get ID and Password
                    string sql = "Select id, pwd, blob from Nutzer where nickname=@NAME";
                    using (SqlCommand com = new SqlCommand(sql, con))
                    {
                        com.Parameters.Add("@NAME", System.Data.SqlDbType.NVarChar);
                        com.Parameters["@NAME"].Value = user.name;
                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                request.Errorcode = -1;
                                throw new Exception("No User");
                            }
                            user.id = reader.GetInt32(0);
                            user.storageBlob = reader.GetString(2);
                            user.storageConnect = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                            pwdhash = reader.GetString(1);
                        }
                    }

                    // Check Password
                    if (!BCrypt.Net.BCrypt.CheckPassword(user.name + pass, pwdhash))
                    {
                        request.Errorcode = -2;
                        throw new Exception("Wrong Password");
                    }

                    // Generate TOKEN
                    string AlphaNum = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    Random r = new Random();
                    char[] token = new char[50];
                    for (int i = 0; i < token.Length; i++) token[i] = AlphaNum[r.Next(0, AlphaNum.Length - 1)];
                    user.tempToken = new string(token);

                    // Write TOKEN into Database
                    sql = "Update Nutzer Set token=@TOKEN, tokenAge=@TOKENAGE where id=@ID";
                    using (SqlCommand com2 = new SqlCommand(sql, con))
                    {
                        com2.Parameters.Add("@TOKEN", System.Data.SqlDbType.Char);
                        com2.Parameters.Add("@TOKENAGE", System.Data.SqlDbType.DateTime);
                        com2.Parameters.Add("@ID", System.Data.SqlDbType.Int);

                        com2.Parameters["@TOKEN"].Value = user.tempToken;
                        com2.Parameters["@TOKENAGE"].Value = System.DateTime.Now;
                        com2.Parameters["@ID"].Value = user.id;

                        com2.ExecuteNonQuery();
                    }
                }

                user.loggedIn = true;
                request.Success = true;
            }
            catch (SqlException e)
            {
                request.Success = false;
                request.Error = e.Message;
                request.Errorcode = e.ErrorCode;

            }
            catch (Exception e)
            {
                request.Success = false;
                request.Error = e.Message;
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(request))
            };
        }
    }
}
