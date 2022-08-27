using Bermuda.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.Xaml.Media.Imaging;

namespace Bermuda.Interfaces
{
    public interface IDataService
    {
        
        bool ImportUserData();
        bool SaveUserData();

        // Proxy
        Collection<ProxyGroup> LoadProxyGroups();
        bool SaveProxyGroup(ProxyGroup group);
        bool RemoveProxyGroup(ProxyGroup group);

        // Profile
        Collection<ProfileGroup> LoadProfileGroups();
        bool SaveProfileGroup(ProfileGroup group);
        bool RemoveProfileGroup(ProfileGroup group);

        // Task
        Collection<TaskGroup> LoadTaskGroups();
        bool SaveTaskGroups(TaskGroup group);
        bool RemoveTaskGroup(TaskGroup group);

        // States
        Collection<TaskGroup> GetTaskGroupStates();
        void UpdateTaskGroupState(Collection<TaskGroup> stateCollection);

        // Checkouts
        Collection<CheckoutModel> GetCheckouts();
        void SaveCheckout(CheckoutModel checkout);

        // License
        void SubscribeLicensed(EventHandler<bool> eventHandler);
        bool SaveLicense(string licenseKey);
        string GetLicense();

        // Settings
        bool SaveSettings(SettingsModel settings);
        SettingsModel GetSettings();

        // Updates
        bool SaveUpdateRecord(string update);
        string GetUpdateRecord();

    }
}
