using Gehirn;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using static GehirnJogging.Code.Nutzer;

namespace GehirnJogging.Code
{
    public class StorageAccountManager
    {

        private Nutzer nutzer;
        private CloudStorageAccount cStorageAccount;
        private CloudBlobClient cbClient;
        private CloudBlobContainer cbContainer;
        private StorageFolder puzzleFolder;

        private bool isDownloading;
        private Action<StorageFile> downloadAction;
        private Action downloadEndedAction;

        public bool IsDownloading { get => isDownloading; }
        public Action<StorageFile> DownloadAction { get => downloadAction; set => downloadAction = value;  }
        public Action DownloadEndedAction { get => downloadEndedAction; set => downloadEndedAction = value; }

        private StorageAccountManager() { }

        public static StorageAccountManager FixForOfflineMode()
        {
            StorageAccountManager st = new StorageAccountManager();
            try { st.puzzleFolder = Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Puzzlebilder").GetResults(); }
            catch { st.puzzleFolder = Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFolderAsync("Puzzlebilder").GetResults(); }
            return st;
        }
        public static async Task<StorageAccountManager> CreateAsync(string blob, string connectionString, Nutzer nutzer)
        {
            StorageAccountManager st = new StorageAccountManager();
            try { st.puzzleFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Puzzlebilder"); }
            catch { st.puzzleFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFolderAsync("Puzzlebilder"); }

            if (App.OFFLINE_MODE) return st;

            CloudStorageAccount.TryParse(connectionString, out st.cStorageAccount);
            st.cbClient = st.cStorageAccount.CreateCloudBlobClient();
            st.cbContainer = st.cbClient.GetContainerReference(blob);
            st.nutzer = nutzer;
            return st;
        }

        public async void UploadFileAsync(ISet<StorageFile> files)
        {
            if (files.Count == 0) return;
            ISet<String> filenames = new HashSet<String>();
            
            CloudBlockBlob cloudBlockBlob;
            foreach (StorageFile file in files)
            {
                cloudBlockBlob = cbContainer.GetBlockBlobReference(file.Name);
                filenames.Add(file.Name);
                await cloudBlockBlob.UploadFromFileAsync(file.Path);
            }

            Request<ISet<String>> request = new Request<ISet<String>>();
            request.obj = filenames;
            await request.HttpRequestAsync(App.FUNCTION_SET_IMAGES_FILES);
        }


        public async void DownloadFilesAsync()
        {
            if (isDownloading) return;
            isDownloading = true;
            Request<ISet<String>> request = new Request<ISet<String>>();
            await request.HttpRequestAsync(App.FUNCTION_GET_IMAGES_FILES);

            if (!request.Success) return;
            try { foreach (StorageFile file in puzzleFolder.GetFilesAsync().GetResults()) if (!request.obj.Contains(file.Name)) await file.DeleteAsync(); } catch { };
            await Download(request);
        }

        private async Task Download(Request<ISet<String>> request)
        {
            CloudBlockBlob cloudBlockBlob;
            foreach (String filename in request.obj)
            {
                if (nutzer.LoggendIn == false) break;
                if (await CheckIfFileExistsAsync(filename)) continue;

                cloudBlockBlob = cbContainer.GetBlockBlobReference(filename);
                await cloudBlockBlob.FetchAttributesAsync();
                byte[] content = new byte[cloudBlockBlob.Properties.Length];
                for (int i = 0; i < content.Length; i++) content[i] = 0x20;
                await cloudBlockBlob.DownloadToByteArrayAsync(content, 0);

                try
                {               
                    StorageFile file = await DownloadsFolder.CreateFileAsync(filename + System.DateTime.Now.ToString().Replace(":", "").Replace(".", ""));
                    await Windows.Storage.FileIO.WriteBytesAsync(file, content);
                    StorageFile copy = await file.CopyAsync(puzzleFolder);
                    await copy.RenameAsync(filename);
                    await file.DeleteAsync();
                    if (downloadAction != null) downloadAction.Invoke(copy);
                }catch(Exception e) { Debug.WriteLine(e.Message); }
            }
            isDownloading = false;
            if (downloadEndedAction != null) downloadEndedAction.Invoke();
        }

        private async Task<bool> CheckIfFileExistsAsync(string filenames)
        {
            try
            {
                await puzzleFolder.GetFileAsync(filenames);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async void DeleteFileAsync(ISet<StorageFile> files)
        {
            if (files.Count == 0) return;
            ISet<String> filenames = new HashSet<String>();

            foreach (StorageFile file in files)
            {
                filenames.Add(file.Name);
                try { await file.DeleteAsync(); } catch { }
            }
       
            Request<ISet<String>> request = new Request<ISet<String>>();
            request.obj = filenames;
            await request.HttpRequestAsync(App.FUNCTION_DEL_IMAGES_FILES);
        }

        public async void GetStandardImagesAsync()
        {
            if (isDownloading) return;
            isDownloading = true;
            Request<ISet<String>> request = new Request<ISet<string>>();
            await request.HttpRequestAsync(App.FUNCTION_SET_STANDARD_IMAGE_FILES);
            if(!request.Success || request.obj.Count==0)
            {
                isDownloading = false;
                downloadEndedAction.Invoke();
            }
            else await Download(request);
        }
    }

}
