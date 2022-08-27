using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Bermuda.Interfaces;
using System.Net.Http;
using System.Text.Json;
using Bermuda.Models;
using Windows.Web;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.Storage;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.Vpn;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Numerics;

namespace Bermuda.Services
{

    class CommunicationHandler
    {

        const string REMOTE_HOST = "localhost";
        const int API_SERVICE_PORT = 10001;
        const int SOCKET_SERVICE_PORT = 10002;
        const string BERMUDA_API = "https://www.bermudabot.com";

        private DataService _dataService;
        public event EventHandler<TaskUpdate> TaskUpdateReceived;
        public event EventHandler<CheckoutUpdate> CheckoutUpdateReceived;
        public event EventHandler<int> DownloadProgress;
        public event EventHandler<bool> BackendConnected;

        public Dictionary<string, Collection<TaskUpdate>> savedUpdates;

        public CommunicationHandler(DataService dService)
        {
            _dataService = dService;
            this.savedUpdates = new Dictionary<string, Collection<TaskUpdate>>();
            StartReceivingLayer();
        }

        public void StartReceivingLayer()
        {

            Thread connector = new Thread(new ThreadStart(() =>
            {

                const Int32 RemotePort = 10002;
                TcpClient client = new TcpClient();
                bool clientConnected = false;

                while(!clientConnected)
                {
                    client = new TcpClient();
                 
                    try
                    {
                        client.Connect(REMOTE_HOST, SOCKET_SERVICE_PORT);
                    }
                    catch(Exception err){}

                    if (client.Connected)
                    {
                        this.BackendConnected?.Invoke(this, true);
                        clientConnected = true;
                    }

                    Thread.Sleep(1000);
                }

                NetworkStream stream = client.GetStream();
                int i = 0;
                byte[] buffer = new byte[1024 * 10];
                
                while((i = stream.Read(buffer, 0, buffer.Length))!=0)
                {
                    string data = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    Debug.WriteLine($"Got new update {data}");
                    string[] lines = data.Split("*STREAM_DELIM*");
                    Debug.WriteLine($"Number of lines{lines.Length}");
                   
                    foreach(string line in lines)
                    {
                        if (line.Length > 0)
                        {
                            this._ProcessEvent(line);
                        }
                    }

                }

            }));

            connector.Start();

        }
        
        public async System.Threading.Tasks.Task<int> ValidateLicenseKey(string licenseKey)
        {
            
            HttpClient client = new HttpClient();
            var res = await client.PostAsync($"http://{REMOTE_HOST}:{API_SERVICE_PORT}/api/validate?licenseKey={licenseKey}", null);
            return (int)res.StatusCode;
        }

        public void StartTaskGroup(string jsonTaskGroup)
        {
            HttpClient client = new HttpClient();
            StringContent reqBody = new StringContent(jsonTaskGroup);
            TaskGroup tg = JsonSerializer.Deserialize<TaskGroup>(jsonTaskGroup);
            this._CreateTaskHistroy(tg);
            var res = client.PutAsync($"http://{REMOTE_HOST}:{API_SERVICE_PORT}/api/task_group", reqBody);
        }

        public void StopTaskGroup(string jsonTaskGroup)
        {
            HttpClient client = new HttpClient();
            StringContent reqBody = new StringContent(jsonTaskGroup);
            TaskGroup tg = JsonSerializer.Deserialize<TaskGroup>(jsonTaskGroup);
            this._RemoveTaskHistory(tg);
            var res = client.PostAsync($"http://{REMOTE_HOST}:{API_SERVICE_PORT}/api/task_group/delete", reqBody); 
        }

        public void ConfigureSettings(string jsonSettings)
        {
            HttpClient client = new HttpClient();
            StringContent reqBody = new StringContent(jsonSettings);
            var res = client.PostAsync($"http://{REMOTE_HOST}:{API_SERVICE_PORT}/api/settings", reqBody);
        }

        public async System.Threading.Tasks.Task<string> CheckNeedUpdate()
        {
            HttpClient client = new HttpClient();
            var res = await client.GetAsync($"{BERMUDA_API}/api/update/hash");
            string body = await res.Content.ReadAsStringAsync();
            return body;
        }

        private void _CreateTaskHistroy(TaskGroup taskGroup)
        {
            Collection<TaskUpdate> taskHistory = new Collection<TaskUpdate>();
            for (int x = 0; x < taskGroup.numThreads; x++)
            {
                TaskUpdate tUpdate = new TaskUpdate
                {
                    TaskThreadId = x.ToString(),
                    TaskGroupId = taskGroup.groupName
                };
                taskHistory.Add(tUpdate);
            }

            this.savedUpdates.Add(taskGroup.groupName, taskHistory);
        }

        private void _RemoveTaskHistory(TaskGroup taskGroup)
        {
            if (this.savedUpdates.ContainsKey(taskGroup.groupName))
            {
                this.savedUpdates.Remove(taskGroup.groupName);
            }
        }

        private void _UpdateTaskHistory(TaskUpdate taskUpdate)
        {

            string foundGroup = "";
            bool hasFoundGroup = false;

            foreach (string taskGroupName in this.savedUpdates.Keys)
            {
                if (taskGroupName == taskUpdate.TaskGroupId)
                {
                    foundGroup = taskGroupName;
                    hasFoundGroup = true;
                    break;
                }
            }

            if (hasFoundGroup && this.savedUpdates.ContainsKey(foundGroup))
            {
                this.savedUpdates[foundGroup][int.Parse(taskUpdate.TaskThreadId)] = taskUpdate;
            }

        }

        public void _SendStoredUpdates()
        {
            foreach(Collection<TaskUpdate> taskSavedUpdates in this.savedUpdates.Values)
            {
                foreach(TaskUpdate taskUpdate in taskSavedUpdates)
                {
                    this.TaskUpdateReceived?.Invoke(this, taskUpdate);
                }
            }
        }

        private void _ProcessEvent(string jsonEvents)
        {

            string[] splitEvents = jsonEvents.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            foreach (string strEvent in splitEvents)
            {

                try
                {
                    EventContainer backendEvent = JsonSerializer.Deserialize<EventContainer>(strEvent);

                    switch (backendEvent.eventType)
                    {
                        case "TaskUpdate":
                            TaskUpdate taskUpdate = JsonSerializer.Deserialize<TaskUpdate>(backendEvent.eventDataJson);
                            this._UpdateTaskHistory(taskUpdate);
                            this.TaskUpdateReceived?.Invoke(this, taskUpdate);
                        break;
                        case "CheckoutUpdate":
                            CheckoutUpdate checkoutUpdate = JsonSerializer.Deserialize<CheckoutUpdate>(backendEvent.eventDataJson);
                            this.CheckoutUpdateReceived?.Invoke(this, checkoutUpdate);
                            this._SaveCheckoutUpdate(checkoutUpdate);
                        break;
                    }

                }
                catch (Exception err)
                {}

            }
        
            return;
        }

        private void _SaveCheckoutUpdate(CheckoutUpdate update)
        {

            CheckoutModel uCheckout = new CheckoutModel
            {
                ProfileName = update.ProfileName,
                Size = update.ProductSize,
                SKU = update.ProductSKU,
                ProductImage = update.ProductImage
            };

            _dataService.SaveCheckout(uCheckout);

        }

        public async System.Threading.Tasks.Task<string> DownloadLatestUpdate()
        {

            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();
            var uri = new System.Uri($"{BERMUDA_API}/api/update");

            Debug.WriteLine("Downloading: ", uri);

            var request = client.GetAsync(uri);
            request.Progress += (item, progress) =>
             {
                 if (progress.BytesReceived != 0)
                 {
                     int totalDownloadMB = (int)(progress.BytesReceived / 1024 / 1024);
                     int totalFilesizeMB = (int)(progress.TotalBytesToReceive / 1024 / 1024);
                     if (totalDownloadMB != 0)
                     {
                         Debug.WriteLine($"Total Downloaded {totalDownloadMB}, total size {totalFilesizeMB}, {totalDownloadMB/totalFilesizeMB}");
                         int downloadPercentage = (int)(((double)totalDownloadMB / (double)totalFilesizeMB) * 100);
                         
                         DownloadProgress?.Invoke(this, downloadPercentage);
                     }
                 }
            };

            var res = await request;

            // Save the update to disk
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string updateLocation = $"{localFolder.Path}\\BermudaAIO.exe";
            if (File.Exists(updateLocation))
            {
                File.Delete(updateLocation);
            }
            var updateBuffer = await res.Content.ReadAsBufferAsync();
            File.WriteAllBytes(updateLocation, updateBuffer.ToArray());

            return updateLocation;

        }

    }

    class BackendService : IBackendService
    {

        private CommunicationHandler _comHandler;
        private DataService _dataService;
        public BackendService(DataService dataService)
        {
            _dataService = dataService; 
            Connect();
        }

        public bool Connect()
        {
            this._comHandler = new CommunicationHandler(
                this._dataService
            );
            return true;
        }
        
        public bool Disconnect()
        {
            return true;
        }

        public bool UnsubscribeTaskUpdates(EventHandler<TaskUpdate> eventHandler)
        {
            _comHandler.TaskUpdateReceived -= eventHandler;
            return true;
        }

        public bool ForwardTaskUpdates(EventHandler<TaskUpdate> eventHandler)
        {

            // Foward updates to event handler
            _comHandler.TaskUpdateReceived += eventHandler;
            _comHandler._SendStoredUpdates();

            return true;
        }

        public Collection<TaskUpdate> GetSavedUpdates(TaskGroup taskGroup)
        {

            if (this._comHandler.savedUpdates.ContainsKey(taskGroup.groupName))
            {
                return this._comHandler.savedUpdates[taskGroup.groupName];
            }

            throw new Exception("No saved updates");

        }

        public bool ForwardCheckoutUpdates(EventHandler<CheckoutUpdate> eventHandler)
        {
            _comHandler.CheckoutUpdateReceived += eventHandler;
            return true;
        }

        public bool UnsubscribeCheckoutUpdates(EventHandler<CheckoutUpdate> eventHandler)
        {
            _comHandler.CheckoutUpdateReceived -= eventHandler;
            return true;
        }

        public bool StartTaskGroup(string strTaskGroup) {
            this._comHandler.StartTaskGroup(strTaskGroup);
            return true;
        }

        public bool StopTaskGroup(string groupId) {
            this._comHandler.StopTaskGroup(groupId);
            return true;
        }

        public System.Threading.Tasks.Task<int> ValidateLicense(string licenseKey)
        {
            return this._comHandler.ValidateLicenseKey(licenseKey);
        }

        public async System.Threading.Tasks.Task<bool> DownloadLatestUpdate(EventHandler<int> eventHandler)
        {

            this._comHandler.DownloadProgress += eventHandler;
            try
            {
                var fileBytes = await this._comHandler.DownloadLatestUpdate();
                // Save to disk
            }catch(Exception err)
            {
                return false;
            }

            this._comHandler.DownloadProgress -= eventHandler;

            string latestUpdate = await this._comHandler.CheckNeedUpdate();
            this._dataService.SaveUpdateRecord(latestUpdate);

            return true;
        }

        public async System.Threading.Tasks.Task<bool> CheckUpdate()
        {
            string latestUpdate = await this._comHandler.CheckNeedUpdate();
            string savedUpdate = this._dataService.GetUpdateRecord();

            return latestUpdate != savedUpdate;
        }

        public bool BackendOnlineEvent(EventHandler<bool> eventHandler)
        {
            this._comHandler.BackendConnected += eventHandler;
            return true;
        }

        public bool ConfigureSettings(string settings)
        {
            this._comHandler.ConfigureSettings(settings);
            return true;
        }


    }
}
