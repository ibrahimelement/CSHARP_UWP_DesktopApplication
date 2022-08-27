using Bermuda.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Core;

namespace Bermuda.ViewModels
{
    public class LicenseViewModel : BindableBase
    {

        public string storedLicenseKey;

        private bool _hasLicenseKey = false;
        public bool hasLicenseKey
        {
            get
            {
                return _hasLicenseKey;
            }
            set
            {
                _hasLicenseKey = value;
                OnPropertyChanged();
            }
        }

        private int _updateProgress = 0;
        public int UpdateProgress
        {
            get
            {
                return _updateProgress;
            }
            set
            {
                _updateProgress = value;
                OnPropertyChanged();
            }
        }


        public LicenseViewModel(IDataService dataService, INavigationService navigationService, IBackendService backendService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _backendService = backendService;
            _PopulateData();
        }

        public async Task<Boolean> _HandleUpdate()
        {

            bool updateAvailable = await _backendService.CheckUpdate();
            if (updateAvailable)
            {
                EventHandler<int> eventHandler = (Object sender, int update) =>
                {
                    _UpdateDownloadProgress(update);
                };
                await _backendService.DownloadLatestUpdate(eventHandler);
            }

            return true;

        }

        public async Task<Boolean> _SpawnBackend()
        {

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            EventHandler<bool> eventHandler = (Object sender, bool update) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    tcs.SetResult(update);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                
            };
            _backendService.BackendOnlineEvent(eventHandler);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }

            await tcs.Task;

            return true;
        }

        private void _UpdateDownloadProgress(int progress)
        {
            // https://stackoverflow.com/questions/19341591/the-application-called-an-interface-that-was-marshalled-for-a-different-thread
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateProgress = progress;
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        }

        public async Task<bool> ValidateLicense(string licenseKey)
        {

            int statusCode = await _backendService.ValidateLicense(licenseKey);
            if (statusCode == 200)
            {
                _dataService.SaveLicense(licenseKey);
                return true;
            }
            return false;
        }

        public void _DevelopmentSaveLicense(string licenseKey)
        {
            _dataService.SaveLicense(licenseKey);
        }

        private void _PopulateData()
        {

            string licenseKey = _dataService.GetLicense();
            if (licenseKey != null && licenseKey.Length > 0)
            {
                hasLicenseKey = true;
                storedLicenseKey = licenseKey;
            }

        }


    }
}
