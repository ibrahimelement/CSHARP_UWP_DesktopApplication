using Bermuda.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Core;

namespace Bermuda.ViewModels
{
    public class MainViewModel : BindableBase
    {

        private string _pageHeaderText;
        public string PageHeaderText
        {
            get
            {
                return _pageHeaderText;
            }
            set
            {
                _pageHeaderText = value;
                OnPropertyChanged();
            }
        }

        private bool _navigationEnabled = true;
        public bool NavigationEnabled
        {
            get
            {
                return _navigationEnabled;
            }
            set
            {
                _navigationEnabled = value;
                OnPropertyChanged();
            }
        }

        //public string PageHeaderText = "Developer Page";

        public ProxyViewModel ProxyView { get; } = (Application.Current as App).Container.GetService<ProxyViewModel>();

        public MainViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            Setup();
        }

        public void DisableNavigation()
        {
            NavigationEnabled = false;
        }

        public void EnableNavigation()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationEnabled = true;
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed   
        }

        public void Setup()
        {
            PageHeaderText = "Default";
            SubscribeLicenseEvent();
        }

        private void SubscribeLicenseEvent()
        {
            // This is only triggered when we decide to save the license, which is always right after processing
            // and successful validation (every reboot).
            EventHandler<bool> eventHandler = (Object sender, bool data) =>
            {
                EnableNavigation();
                TaskOverviewPressed();
            };
            _dataService.SubscribeLicensed(eventHandler);
        }

        public void SetFrame(Frame t)
        {
            _navigationService.SetFrame(t);
            //TaskOverviewPressed();
        }

        public void TaskOverviewPressed()
        {
            PageHeaderText = "Task Overview and Control";
            this._navigationService.NavigateTo("TaskOverview");
        }

        public void ProxiesPressed()
        {
            PageHeaderText = "Proxy Creation and Management";
            this._navigationService.NavigateTo("Proxies");
        }

        public void ProfilesPressed()
        {
            PageHeaderText = "Profile Creation and Management";
            this._navigationService.NavigateTo("Profiles");
        }

        public void CheckoutsPressed()
        {
            PageHeaderText = "Checkouts Overview";
            this._navigationService.NavigateTo("Checkouts");
        }
        
        public void LicensePressed()
        {
            PageHeaderText = "License Management";
            this._navigationService.NavigateTo("License");
        }

        public void SettingsPressed()
        {
            PageHeaderText = "Settings and Integrations";
            this._navigationService.NavigateTo("Settings");
        }

    }
}
