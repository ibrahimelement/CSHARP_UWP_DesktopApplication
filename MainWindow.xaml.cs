using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Bermuda.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Media.Imaging;
using Bermuda.Services;
using System.Reflection;
using System.IO;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Bermuda.Models;
using Bermuda.Interfaces;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Page
    {
        public MainViewModel ViewModel { get; } = (Application.Current as App).Container.GetService<MainViewModel>();
        
        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel.SetFrame(this.contentFrame);
            HandleLicense();
        }

        private void HandleLicense()
        {

            // Check if licensed
            /*
            ViewModel.DisableNavigation();
            ViewModel.LicensePressed();
            */
            ViewModel.TaskOverviewPressed();

        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            
            switch(args.InvokedItem)
            {
                case "Task Overview":
                    ViewModel.TaskOverviewPressed();
                    break;
                case "Manage Proxies":
                    ViewModel.ProxiesPressed();
                    break;
                case "Manage Profiles":
                    ViewModel.ProfilesPressed();
                    break;
                case "Checkout Overview":
                    ViewModel.CheckoutsPressed();
                    break;
                case "Settings":
                    ViewModel.SettingsPressed();
                    break;
            }

            if (args.IsSettingsInvoked)
            {
                ViewModel.SettingsPressed();
            }

        }

    }
}
