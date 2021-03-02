using Gehirn;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace GehirnJogging.Code
{
    /* NUTZER */
    public class Nutzer
    {
        private static Nutzer instance;
        public static Nutzer Getinstance()
        {
            if (instance == null)
            {
                instance = new Nutzer();
                if (App.OFFLINE_MODE) { }
            }

            
            return instance;
        }
        private Nutzer() {
            if (App.OFFLINE_MODE)
            {
                name = "Testnutzer";
                id = 0;
                loggendIn = true;
                stManager = StorageAccountManager.CreateAsync("", "", this).Result;
            }
        }


        private int id;
        private string name;
        private string tempToken;
        private string storageConnect;
        private string storageBlob;
        private bool loggendIn = false;
        private StorageAccountManager stManager;

        public int Id { set => id = value; }
        public string Name { get => name; set => name = value; }
        public string TempToken { set => tempToken = value;  }
        public string StorageConnect { set => storageConnect = value; }
        public string StorageBlob { set => storageBlob = value; }
        public bool LoggendIn { get => loggendIn;  }
        public StorageAccountManager StManager { get => stManager; }


        public static async Task<Request<Nutzer>> Einloggen(string aktion, string name, string pwd)
        {
            if (instance != null) instance.loggendIn = false;
            instance = new Nutzer();
            instance.name = name;
            instance.tempToken = pwd;

            Request<Nutzer> request = new Request<Nutzer>();
            request.obj = instance;

            await request.HttpRequestAsync(aktion);

            if (request.Success)
            {
                instance.name =       request.obj.name;
                instance.id =         request.obj.id;
                instance.tempToken =  request.obj.tempToken;
                instance.loggendIn =  true;
                instance.stManager = await StorageAccountManager.CreateAsync(request.obj.storageBlob, request.obj.storageConnect, instance);
                instance.stManager.DownloadFilesAsync();
            }
            return request;
        }

        public void Abmelden()
        {
            loggendIn = false;
            tempToken = "";
        }

        /* REQUEST */
        public class Request<T>
        {

            private readonly string TOKEN = Getinstance().tempToken;
            private int id = Getinstance().id;
            private bool success;
            private int errorcode;
            private string error;

            public int Id { get => id; set => id = value; }
            public bool Success { get => success; set => success = value; }
            public int Errorcode { get => errorcode; set => errorcode = value; }
            public string Error { get => error; set => error = value; }
            public T obj;

            private string Serialize() => "{\"TOKEN\":" + "\"" + TOKEN + "\"," + JsonConvert.SerializeObject(this).Substring(1);
            private Request<T> Deserialize(string json) => JsonConvert.DeserializeObject<Request<T>>(json);

            public async Task HttpRequestAsync(String function)
            {
                using (HttpClient http = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage re = http.PostAsync(App.FUNCTION_URI + function + App.FUNCTION_KEY, new StringContent(this.Serialize())).Result;
                        Request<T> answer = this.Deserialize(await re.Content.ReadAsStringAsync());
                        this.success = answer.success;
                        this.errorcode = answer.errorcode;
                        this.error = answer.error;
                        this.obj = answer.obj;
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        if(Nutzer.Getinstance().loggendIn) App.Abmelden("Die Verbindung zur Datenbank wurde abgebrochen");
                        Errorcode = -3;
                    }
                }
                if (!success && errorcode == -10) App.Abmelden("Ihr Token ist abgelaufen");
            }
        }
    }

}
