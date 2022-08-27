using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Bermuda.ViewModels;
using Windows.UI.Xaml;
using System.ComponentModel;
using Bermuda.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {

        public SettingsViewModel SettingsView { get; } = (Application.Current as App).Container.GetService<SettingsViewModel>();
        
        public Settings()
        {
            this.InitializeComponent();
            PopulateCollections();
        }

        private bool hasLoaded = false;

        private void PopulateCollections()
        {   
            
            if (SettingsView.proxyGroups != null)
            {
                ProxySelectionDrpDwn.IsEnabled = true;
                foreach (ProxyGroup pg in SettingsView.proxyGroups)
                {
                    MenuFlyoutItem item = new MenuFlyoutItem();
                    item.Text = item.Text = pg.GroupName;
                    item.Click += (object sender, RoutedEventArgs e) =>
                    {
                        SettingsView.chosenProxyGroup = pg;
                        BrowserProxiesLbl.Text = "Browser Proxies: " + pg.Proxies.Count;
                    };
                    ProxyGroupMenuFlyout.Items.Add(
                        item
                    );
                }

                double val = SettingsView.numBrowsers;
                NumBrowsersSld.Value = val;
                this.hasLoaded = true;

            }else
            {

            }
            
        }

        private async void NotifyUser(bool success, string title, string status)
        {
            InfoCommandStatus.Visibility = Visibility.Collapsed;
            InfoCommandStatus.IsOpen = false;
            InfoCommandStatus.Title = title;
            InfoCommandStatus.Message = status;
            InfoCommandStatus.Severity = success ?
                Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success :
                Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            InfoCommandStatus.Opacity = 0;
            InfoCommandStatus.IsOpen = true;
            InfoCommandStatus.Visibility = Visibility.Visible;
            InfoCommandStatus.Opacity = 1;
        }

        private void SaveSettingsBtn_Click(object sender, RoutedEventArgs e)
        {

            if (SettingsView.chosenProxyGroup != null)
            {
                SettingsView.SaveConfiguration();
                NotifyUser(true, "Saved", "Updated settings");
                return;
            }
            
            NotifyUser(false, "Error", "Please setup or select a proxy group");

        }

        private void NumBrowsersSld_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (this.hasLoaded)
            {
                SettingsView.numBrowsers = (int)e.NewValue;
            }
        }
    }
}
