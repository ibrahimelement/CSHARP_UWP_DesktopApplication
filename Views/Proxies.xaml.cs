using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Bermuda.ViewModels;
using Bermuda.Models;
using System.Runtime.InteropServices.WindowsRuntime;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Proxies : Page
    {
        public ProxyViewModel ProxyView { get; } = (Application.Current as App).Container.GetService<ProxyViewModel>();

        public Proxies()
        {
            this.InitializeComponent();
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

        private async void ProxyGroupList_Add_Click(object sender, RoutedEventArgs e)
        {
            
            // Create dialog
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Create a new Group";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";

            // Create form
            TextBlock formTitle = new TextBlock();
            TextBox groupName = new TextBox();
            StackPanel formPanel = new StackPanel();
            TextBlock blockTitle = new TextBlock();
            TextBox pasteBox = new TextBox();
            
            formTitle.Text = "Group Name:";
            groupName.PlaceholderText = "NikeProxies1";
            blockTitle.Text = "Paste below:";
            pasteBox.AcceptsReturn = true;
           
            for (int x = 0; x < 5; x++)
            {
                pasteBox.PlaceholderText += $"proxies.boroinc.com:{10000 + x}:username:password\n";
            }

            formPanel.Orientation = Orientation.Vertical;
            formPanel.Children.Add(formTitle);
            formPanel.Children.Add(groupName);
            formPanel.Children.Add(blockTitle);
            formPanel.Children.Add(pasteBox);

            // Add to dialog
            dialog.Content = formPanel;

            var result = await dialog.ShowAsync();

            // If the user press save
            if (result == ContentDialogResult.Primary)
            {

                // Take info from the user
                string strGroupName = groupName.Text.ToString();
                string strProxies = pasteBox.Text.ToString();
                bool successfulInput = true;

                // Validate inputs
                if (strGroupName.Length == 0)
                {
                    successfulInput = false;
                } else if (strProxies.Length == 0)
                {
                    successfulInput = false;
                }


                int totalImported = 0;
               
                if (successfulInput)
                {
                    try
                    {
                        totalImported = this.ProxyView.AddGroup(strGroupName, strProxies);
                        
                    }
                    catch (Exception err) {}
                }

                // Inform user of importation status
                if (successfulInput && totalImported > 0)
                {
                    NotifyUser(successfulInput, "Add Proxy", $"Sucessfully imported {totalImported} into new Group");
                } else
                {
                    NotifyUser(successfulInput, "Add Proxy", "Failed to add group, please ensure that data is valid!");
                }


            }

        }

        private void ProxyGroupList_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ProxyGroupList.SelectedIndex < 0)
            {
                NotifyUser(false, "Remove Group", "No group selected");
            }else
            {
                bool successfulDeletion = this.ProxyView.RemoveGroup((ProxyGroupList.SelectedItem as ProxyGroup).GroupName);
                if (ItemsGridView.Visibility == Visibility.Visible)
                {
                    ItemsGridView.Visibility = Visibility.Collapsed;
                    EmptyProxiesStack.Visibility = Visibility.Visible;
                }
                if (successfulDeletion)
                {
                    NotifyUser(true, "Remove Group", "Successfuly removed the group");
                }
            }
        }

        private void ProxyGroupList_ItemClick(object sender, ItemClickEventArgs e)
        {
       
            this.ProxyView.SelectGroup((e.ClickedItem as ProxyGroup).GroupName);
            if (ItemsGridView.Visibility == Visibility.Collapsed)
            {
                EmptyProxiesStack.Visibility = Visibility.Collapsed;
                ItemsGridView.Visibility = Visibility.Visible;
            }

        }

        private void ItemsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)ItemsGridView.ItemsPanelRoot;
            panel.ItemWidth = e.NewSize.Width;
        }


    }
}
