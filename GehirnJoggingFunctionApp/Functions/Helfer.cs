using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace AzureFunctions
{
    public class Request<T>
    {
        public string TOKEN;
        public int Id;
        public bool Success;
        public int Errorcode;
        public string Error;
        public T obj;
    }

    public class User
    {
        public int id;
        public string name;
        public string storageBlob;
        public string storageConnect;
        // public string pwd;
        public string tempToken;
        public bool loggedIn = false;
    }

    public class SpielHighscores
    {
        public SpielHighscores() => spiele = new Dictionary<int, Spiel>();
        public Dictionary<int, Spiel> spiele = new Dictionary<int, Spiel>();

        public class Spiel
        {
            public int id_spiel;
            public string spiel;
            public int globaleBestzeit;
            public int globalerDurschnitt;
            public string rekordhalter;

            public int bestzeit;
            public int schlechtesteZeit;
            public int durchschnitt;
            public int anzahl_gespielt;
            public Int64 akummulierteSpielzeit;
        }
    }


    public class FUNCTIONS<T>
    {

        public static HttpResponseMessage checkToken(Request<T> request, SqlConnection con)
        {
            // TOKEN überprüfen
            string sql = "Select TOKEN From Nutzer where id=" + request.Id;
            using (SqlCommand com = new SqlCommand(sql, con))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    reader.Read();
                    string tok = null;
                    try
                    {
                        tok = reader.GetString(0);
                    }
                    catch { }

                    if (!request.TOKEN.Equals(tok))
                    {
                        request.Success = false;
                        request.Error = "Falscher Token";
                        request.Errorcode = -10;
                        request.TOKEN = null;
                        return new HttpResponseMessage
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(request))
                        };
                    }
                }
            }
            request.TOKEN = null;
            return null;
        }
    }


    public class FUNCTIONS2
    {

        public static async Task<ISet<String>> GetImages()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
            var client = storageAccount.CreateCloudBlobClient();
            var cbContainer = client.GetContainerReference("images");

            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await cbContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            ISet<String> list = new HashSet<string>();
            foreach (IListBlobItem b in results) list.Add(Regex.Replace(b.StorageUri.PrimaryUri.Segments.Last<string>().ToString(), "%20", " "));
            return list;
        }


        public static async Task CopyImages(CloudBlobContainer source, CloudBlobContainer target, ISet<String> blobs)
        {

            foreach (String blob in blobs)
            {
                var cloudBlockBlob = source.GetBlockBlobReference(blob);
                await cloudBlockBlob.FetchAttributesAsync();
                byte[] content = new byte[cloudBlockBlob.Properties.Length];
                for (int i = 0; i < content.Length; i++) content[i] = 0x20;
                await cloudBlockBlob.DownloadToByteArrayAsync(content, 0);

                cloudBlockBlob = target.GetBlockBlobReference(blob);
                await cloudBlockBlob.UploadFromByteArrayAsync(content, 0, content.Length);
            }
        }


        public static void writeToDb(ISet<String> blobs, int id, SqlConnection con)
        {
            foreach (String blob in blobs)
            {
                // Anfrage bearbeiten
                string sql = "insert imageNutzer values(@ID, @FNAME)";
                using (SqlCommand com1 = new SqlCommand(sql, con))
                {
                    com1.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                    com1.Parameters.Add("@FNAME", System.Data.SqlDbType.NVarChar);

                    com1.Parameters["@FNAME"].Value = blob;
                    com1.Parameters["@ID"].Value = id;

                    com1.ExecuteNonQuery();
                }
            }
        }
    }
}
