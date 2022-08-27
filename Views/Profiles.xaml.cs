using Bermuda.Models;
using Bermuda.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Profiles : Page
    {

        public ProfileViewModel ProfileView { get; } = (Application.Current as App).Container.GetService<ProfileViewModel>();

        public Profiles()
        {
            this.InitializeComponent();
        }

        private async void AppBarButton_Add_Click(object sender, RoutedEventArgs e)
        {
            
            ContentDialog dialog = new ContentDialog();

            dialog.Title = "Create a new Profile Group";
            dialog.PrimaryButtonText = "Import";
            dialog.CloseButtonText = "Cancel";

            // Create form
            TextBlock formTitle = new TextBlock();
            TextBox groupName = new TextBox();
            StackPanel formPanel = new StackPanel();

            // Set task group name
            formTitle.Text = "Group Name:";
            groupName.PlaceholderText = "MyProfileGroup1";

            /*
             * At the moment, we want to offer two different ways of inputting data
             * 1. Paste with the correct format
             * 2. Create a form with all the proper inputs
             * 
             * We will use two expanders with radio buttons
             */

            TextBlock pasteProfileBlock = new TextBlock();
            TextBox pasteProfilesBox = new TextBox();

            pasteProfileBlock.Text = "Paste profiles:";
            pasteProfilesBox.TextWrapping = TextWrapping.Wrap;
            pasteProfilesBox.AcceptsReturn = true;
            pasteProfilesBox.PlaceholderText = "Format: profile name, first name, last name, address 1, address 2, city, state/province, state/province code, zip, card, expM, expY, cvv, email, phone#";
           
            // Add all items to the stack panel
            formPanel.Children.Add(formTitle);
            formPanel.Children.Add(groupName);
            formPanel.Children.Add(pasteProfileBlock);
            formPanel.Children.Add(pasteProfilesBox);

            // Add stack panel to a scroll viewer
            ScrollViewer formScroller = new ScrollViewer();
            formScroller.Content = formPanel;
            
            // Add to dialag
            dialog.Content = formScroller;

            var res = await dialog.ShowAsync();

            // If the users pressed save
            if (res == ContentDialogResult.Primary)
            {
                
                string strGroupName = groupName.Text.ToString();
                string strProfilesPaste = pasteProfilesBox.Text.ToString();

                
                if (
                    strGroupName.Length == 0 ||
                    strProfilesPaste.Length == 0
                    )
                {
                    NotifyUser(false, "Import Profiles", "Error, missing fields");
                    return;
                }

                int importCount = await ProfileView.ImportProfiles(strGroupName, strProfilesPaste);
                
                if (importCount == 0)
                {
                    NotifyUser(false, "Import Profiles", "Error, incorrect import format");
                    return;
                }

                // Successful
                NotifyUser(true, "Import Profiles", $"Successfully imported {importCount} profiles into group {strGroupName}");

            }


        }

        private void AppBarButton_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (this.ProfileGroupList.SelectedIndex >= 0)
            {
                this.ProfileView.RemoveGroup(ProfileGroupList.SelectedItem as ProfileGroup);
                this.NotifyUser(true, "Remove Group", "Sucessfully removed profile group");
                ProfileContentDisplay.Visibility = Visibility.Collapsed;
                EmptyProfileStack.Visibility = Visibility.Visible;
            }
        }

        private void ProfileGroupList_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ProfileView.SelectGroup((e.ClickedItem as ProfileGroup).GroupName);
            EmptyProfileStack.Visibility = Visibility.Collapsed;
            ProfileContentDisplay.Visibility = Visibility.Visible;
        }

        private void ItemsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)ItemsGridView.ItemsPanelRoot;
            panel.ItemWidth = e.NewSize.Width;
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

    }
}
