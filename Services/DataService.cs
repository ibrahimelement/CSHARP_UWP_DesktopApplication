using Bermuda.Interfaces;
using Bermuda.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Bermuda.Services
{
    public class DataService : IDataService
    {

        public Collection<TaskGroup> taskGroupStates { get; set; }
        public BitmapImage TestBitmap { get; private set; }
        public UserDataModel userData { get; set; }
        private event EventHandler<bool> licensedEventHandler;
        
        public DataService()
        {
            ImportUserData();
        }

        public bool ImportUserData()
        {
            userData = new UserDataModel();
            userData.profileGroups = new Collection<ProfileGroup>();
            userData.proxyGroups = new Collection<ProxyGroup>();
            userData.taskGroups = new Collection<TaskGroup>();
            userData.checkouts = new Collection<CheckoutModel>();
            userData.settings = new SettingsModel();
            userData.licenseKey = "";
            userData.currentBackendVersion = "";
            // Runtime state
            taskGroupStates = new Collection<TaskGroup>();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string configLocation = $"{localFolder.Path}\\UserData.json";

            if (!File.Exists(configLocation))
            {
                FileStream file = File.Create(configLocation);
                file.Close();
                string emptyData = JsonSerializer.Serialize<UserDataModel>(userData);
                File.WriteAllText(configLocation, emptyData);
            }

            string userConfiguration = File.ReadAllText(configLocation);
            userData = JsonSerializer.Deserialize<UserDataModel>(userConfiguration);

            // Need to take into consideration initial missing data.

            return true;
        }

        public bool SaveUserData()
        {
            // Now I should be linking this to the data service via the Add/Remove keywords
            string jsonString = JsonSerializer.Serialize<UserDataModel>(this.userData);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // TODO encryption
            File.WriteAllText($"{localFolder.Path}\\UserData.json", jsonString);
            return true;
        }

        public bool LoadConfiguration()
        {

            return true;
        }

        // Proxy
        public bool SaveProxyGroup(ProxyGroup group)
        {
            this.userData.proxyGroups.Add(group);
            this.SaveUserData();
            return true;
        }

        public bool RemoveProxyGroup(ProxyGroup group)
        {
            this.userData.proxyGroups.Remove(group);
            this.SaveUserData();
            return true;
        }

        public Collection<ProxyGroup> LoadProxyGroups()
        {
            return this.userData.proxyGroups;
        }

        // Profile
        public bool SaveProfileGroup(ProfileGroup group){
            this.userData.profileGroups.Add(group);
            this.SaveUserData();
            return true;
        }

        public bool RemoveProfileGroup(ProfileGroup group){
            this.userData.profileGroups.Remove(group);
            this.SaveUserData();
            return true;
        }

        public Collection<ProfileGroup> LoadProfileGroups()
        {
            return this.userData.profileGroups;
        }

        // Task
        public Collection<TaskGroup> LoadTaskGroups()
        {
            return this.userData.taskGroups;
        }

        public bool SaveTaskGroups(TaskGroup group){
            this.userData.taskGroups.Add(group);
            this.SaveUserData();
            return true;
        }

        public bool RemoveTaskGroup(TaskGroup group)
        {
            this.userData.taskGroups.Remove(group);
            this.SaveUserData();
            return true;
        }

        // Task states (offline)
        public void UpdateTaskGroupState(Collection<TaskGroup> stateCollection)
        {
            this.taskGroupStates.Clear();
            this.taskGroupStates = stateCollection;
        }

        public Collection<TaskGroup> GetTaskGroupStates()
        {
            return this.taskGroupStates;
        }

        public Collection<CheckoutModel> GetCheckouts()
        {
            return this.userData.checkouts;
        }

        public void SaveCheckout(CheckoutModel checkout)
        {
            this.userData.checkouts.Add(checkout);
            this.SaveUserData();
        }

        public void SubscribeLicensed(EventHandler<bool> eventHandler)
        {
            licensedEventHandler += eventHandler;
        }

        public bool SaveLicense(string licenseKey)
        {
            this.userData.licenseKey = licenseKey;
            this.SaveUserData();
            licensedEventHandler?.Invoke(this, true);
            return true;
        }

        public string GetLicense()
        {
            return this.userData.licenseKey;
        }

        public bool SaveUpdateRecord(string update)
        {
            this.userData.currentBackendVersion = update;
            this.SaveUserData();
            return true;
        }

        public string GetUpdateRecord()
        {
            return this.userData.currentBackendVersion;
        }

        public bool SaveSettings(SettingsModel settings)
        {
            this.userData.settings = settings;
            this.SaveUserData();
            return true;
        }

        public SettingsModel GetSettings()
        {
            return this.userData.settings;
        }

    }
}
