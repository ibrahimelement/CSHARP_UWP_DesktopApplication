using Bermuda.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation.Diagnostics;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class License : Page
    {

        public LicenseViewModel LicenseView { get; } = (Application.Current as App).Container.GetService<LicenseViewModel>();

        public License()
        {
            this.InitializeComponent();
            InitTasks();
        }

        private async void InitTasks()
        {
            await LicenseView._HandleUpdate();
            _ShowBackendWait();
            await LicenseView._SpawnBackend();
            _ShowLicense();
            _LoadUserData();
        }

        private void _ShowBackendWait()
        {
            UpdateForm_DownloadProgress.Visibility = Visibility.Collapsed;
            UpdateForm_JobStatus.Text = "Launching BermudaAIO";
            UpdateForm_BackendProgress.Visibility = Visibility.Visible;
        }

        private void _ShowLicense()
        {
            UpdatingForm.Visibility = Visibility.Collapsed;
            LicenseForm.Visibility = Visibility.Visible;
        }

        private void _LoadUserData()
        {
            if (LicenseView.hasLicenseKey)
            {
                TxtLicenseKey.Text = LicenseView.storedLicenseKey;
                _TriggerValidation();
            }
        }

        private async void LicenseBindBtn_Click(object sender, RoutedEventArgs e)
        {
            _TriggerValidation();
        }

        private async void _TriggerValidation()
        {
            string userLicenseKey = TxtLicenseKey.Text;

            if (userLicenseKey.Length > 0)
            {

                LicenseBindBtn.IsEnabled = false;
                TxtLicenseKey.Foreground = new SolidColorBrush(Colors.Lime);
                TxtLicenseKey.Text = "Validating";
                TxtLicenseKey.IsReadOnly = true;

                bool isValid = false;
                try
                {
                    isValid = await LicenseView.ValidateLicense(userLicenseKey);
                }
                catch (Exception error)
                { }

                if (!isValid)
                {
                    TxtLicenseKey.IsReadOnly = false;
                    TxtLicenseKey.Foreground = new SolidColorBrush(Colors.Red);
                    TxtLicenseKey.Text = "Invalid License... try again";
                    TxtLicenseKey.IsReadOnly = true;

                    // Release controls
                    TxtLicenseKey.Foreground = new SolidColorBrush(Colors.White);
                    TxtLicenseKey.IsReadOnly = false;
                    LicenseBindBtn.IsEnabled = true;
                }else
                {
                    LicenseView._DevelopmentSaveLicense(userLicenseKey);
                }

            }

        }

    }
}
