using Bermuda.Interfaces;
using Bermuda.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bermuda.ViewModels
{
    public class SettingsViewModel : BindableBase
    {

        private double _numBrowsers = 10;
        public double numBrowsers
        {
            get
            {
                return _numBrowsers;
            }
            set
            {
                _numBrowsers = value;
                OnPropertyChanged();
            }
        }

        private bool _shareCheckouts = false;
        public bool shareCheckouts
        {
            get
            {
                return _shareCheckouts;
            }
            set
            {
                _shareCheckouts = value;
                OnPropertyChanged();
            }
        }

        string _proxyGroupName = "Select Group";
        public string proxyGroupName
        {
            get
            {
                return _proxyGroupName;
            }
            set
            {
                _proxyGroupName = value;
                OnPropertyChanged();
            }
        }

        private string _twoCaptchaApiKey;
        public string twoCaptchaApiKey {
            get
            {
                return _twoCaptchaApiKey;
            }
            set
            {
                _twoCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<ProxyGroup> proxyGroups;

        public ProxyGroup chosenProxyGroup = null;
        
        public SettingsViewModel(IDataService dataService, INavigationService navigationService, IBackendService backendService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _backendService = backendService;
            LoadConfiguration();
        }

        private bool LoadConfiguration()
        {

            this.proxyGroups = new ObservableCollection<ProxyGroup>();
            Collection<ProxyGroup> proxyGroups = _dataService.LoadProxyGroups();
            SettingsModel settings = _dataService.GetSettings();
            this.numBrowsers = settings.numBrowser == 0 ? 10 : settings.numBrowser;
            this.twoCaptchaApiKey = settings.twoCaptchaApiKey;
            shareCheckouts = settings.shareCheckouts;
            if (settings.browserProxies != null)
            {
                this.proxyGroupName = settings.browserProxies.GroupName;
                this.chosenProxyGroup = settings.browserProxies;
            }
            if (proxyGroups != null)
            {
                foreach (ProxyGroup pg in proxyGroups)
                {
                    this.proxyGroups.Add(pg);
                }
            }

            return true;
        }

        public bool SaveConfiguration()
        {

            SettingsModel settings = new SettingsModel();
            settings.isConfigured = true;
            settings.numBrowser = (int)numBrowsers;
            settings.browserProxies = chosenProxyGroup;
            settings.shareCheckouts = this.shareCheckouts;
            settings.twoCaptchaApiKey = this.twoCaptchaApiKey;

            string strJson = JsonSerializer.Serialize<SettingsModel>(settings);

            _dataService.SaveSettings(settings);
            _backendService.ConfigureSettings(strJson);

            return true;

        }

    }
}
