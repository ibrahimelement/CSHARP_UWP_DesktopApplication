using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Bermuda.ViewModels;
using Bermuda.Models;
using System.Collections.ObjectModel;
using System;
using Microsoft.UI.Xaml.Controls;
using Bermuda.Enums;
using Bermuda.Services;
using Bermuda.Interfaces;
using System.Runtime.CompilerServices;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{

    public sealed partial class TaskOverview : Page
    {

        public TaskViewModel TaskView { get; } = (Application.Current as App).Container.GetService<TaskViewModel>();

        public TaskOverview()
        {
            this.InitializeComponent();
        }

        private string[] ShoeSizes =
        {
            "03.5", "04.0", "04.5", "05.0", "05.5", "06.0", "06.5",
            "07.0","07.5","08.0","08.5","09.0","09.5","10.0","10.5",
            "11.0","11.5","12.0","12.5","13.0","14.0","15.0","16.0","17.0","18.0"
        };

        /* Extremely ugly logic, please optimize in the future...
         * It's making copies of ProxyGroup and ProfileGroup rather than accessing by ID,
         * this could cause business conflicts when remove/add functionality is implemented
         */
        private async void AppBarButton_Add_Click(object sender, RoutedEventArgs e)
        {

            if (!TaskView.hasConfiguredSettings)
            {
                TaskView.NavigateSettings();
                return;
            }

            // Create dialog
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Create a new Task Group";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";

            // Create form
            TextBlock formTitle = new TextBlock();
            TextBox groupName = new TextBox();
            StackPanel formPanel = new StackPanel();

            // Set task group name
            formTitle.Text = "Group Name:";
            groupName.PlaceholderText = "YeezyFootsites1";

            // Site selection
            TextBlock siteSelectionTitle = new TextBlock();
            Expander siteSelectionExpander = new Expander();
            ListBox siteSelectionList = new ListBox();

            siteSelectionTitle.Text = "Site selection:";

            string[] supportedSites = Enum.GetNames(typeof(SupportedSitesEnum));

            foreach(string supportedSite in supportedSites)
            {
                siteSelectionList.Items.Add(supportedSite);
            }

            siteSelectionExpander.Content = siteSelectionList;
            siteSelectionExpander.HorizontalAlignment = HorizontalAlignment.Stretch;
            siteSelectionExpander.Header = "Please select site";
            siteSelectionList.SelectionChanged += (Object tSender, SelectionChangedEventArgs eSender) =>
            {
                string selectedItem = (tSender as ListBox).SelectedItem.ToString();
                siteSelectionExpander.IsExpanded = false;
                siteSelectionExpander.Header = selectedItem;
            };

            // Product Link or SKU
            TextBlock productSpecificationTitle = new TextBlock();
            TextBox productSpecificationInput = new TextBox();

            productSpecificationTitle.Text = "Product link or SKU:";
            productSpecificationInput.PlaceholderText = "C2302100";
           
            // Profiles group expander (need to link to real models)
            TextBlock profileGroupTitle = new TextBlock();
            Expander profileGroupExpander = new Expander();
            ListBox profileGroupList = new ListBox();

            profileGroupExpander.HorizontalAlignment = HorizontalAlignment.Stretch;
            profileGroupTitle.Text = "Select profile group";

            // Load saved profile groups from data service
            Collection<ProfileGroup> savedProfileGroups = this.TaskView.GetProfileGroups();
            foreach(ProfileGroup profileGroup in savedProfileGroups)
            {
                profileGroupList.Items.Add(profileGroup.GroupName);
            }

            if (profileGroupList.Items.Count > 0)
            {
                profileGroupExpander.Header = profileGroupList.Items[0];
                profileGroupList.SelectedItem = profileGroupList.Items[0];
            }
            else
            {
                profileGroupExpander.Header = "No profile groups available";
            }

            profileGroupExpander.Content = profileGroupList;
            profileGroupList.SelectionChanged += (Object tSender, SelectionChangedEventArgs eSender) =>
            {
                string selectedItem = (tSender as ListBox).SelectedItem.ToString();
                profileGroupExpander.IsExpanded = false;
                profileGroupExpander.Header = selectedItem;
            };

            // Proxy group expander (need to link to real models)
            TextBlock proxyGroupTitle = new TextBlock();
            Expander proxyGroupExpander = new Expander();
            ListBox proxyGroupList = new ListBox();

            proxyGroupExpander.HorizontalAlignment = HorizontalAlignment.Stretch;
            proxyGroupTitle.Text = "Select proxy group";
            Collection<ProxyGroup> savedProxyGroups = this.TaskView.GetProxyGroups();

            foreach(ProxyGroup proxyGroup in savedProxyGroups)
            {
                proxyGroupList.Items.Add(proxyGroup.GroupName);
            }
            proxyGroupExpander.Content = proxyGroupList;
            if (proxyGroupList.Items.Count > 0)
            {
                proxyGroupExpander.Header = proxyGroupList.Items[0];
                proxyGroupList.SelectedItem = proxyGroupList.Items[0];
            }else
            {
                proxyGroupExpander.Header = "No proxy groups available";
            }

            proxyGroupList.SelectionChanged += (Object tSender, SelectionChangedEventArgs eSender) =>
            {
                string selectedItem = (tSender as ListBox).SelectedItem.ToString();
                proxyGroupExpander.IsExpanded = false;
                proxyGroupExpander.Header = selectedItem;
            };

            // Size selection expander

            TextBlock sizeSelectionTitle = new TextBlock();
            Expander sizeSelectionExpander = new Expander();
            ListView sizeSelectionList = new ListView();

            sizeSelectionTitle.Text = "Product sizes:";
            sizeSelectionExpander.HorizontalAlignment = HorizontalAlignment.Stretch;

            sizeSelectionExpander.Header = "All Sizes";
            
            // Populate sizes
            for (int x = 0; x < this.ShoeSizes.Length; x += 4)
            {
                StackPanel sizeEntryGroup = new StackPanel();
                sizeEntryGroup.Orientation = Orientation.Horizontal;

                for (int i = 0; i < 4 && i + x < this.ShoeSizes.Length; i++)
                {
                    CheckBox sizeEntry = new CheckBox();

                    sizeEntry.Checked += (Object nSender, RoutedEventArgs ev) =>
                    {
                        sizeSelectionExpander.Header = "Custom";
                    };

                    sizeEntry.Content = $"{this.ShoeSizes[i + x]}";
                    sizeEntryGroup.Children.Add(sizeEntry);
                }

                sizeSelectionList.Items.Add(sizeEntryGroup);
            }
            sizeSelectionExpander.Content = sizeSelectionList;


            // Thread count slider
            TextBlock threadSliderTitle = new TextBlock();
            threadSliderTitle.Text = "Task count:";
            Slider threadSlider = new Slider();
            threadSlider.Minimum = 1;
            threadSlider.Maximum = 250;
            threadSlider.ValueChanged += (object tSender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs eSender) =>
            {
                threadSliderTitle.Text = $"Task count: {(tSender as Slider).Value}";
            };

            // Add all elements to the form
            formPanel.Orientation = Orientation.Vertical;
            formPanel.Children.Add(formTitle);
            formPanel.Children.Add(groupName);
            formPanel.Children.Add(siteSelectionTitle);
            formPanel.Children.Add(siteSelectionExpander);
            formPanel.Children.Add(productSpecificationTitle);
            formPanel.Children.Add(productSpecificationInput);
            formPanel.Children.Add(proxyGroupTitle);
            formPanel.Children.Add(proxyGroupExpander);
            formPanel.Children.Add(profileGroupTitle);
            formPanel.Children.Add(profileGroupExpander);
            formPanel.Children.Add(sizeSelectionTitle);
            formPanel.Children.Add(sizeSelectionExpander);
            formPanel.Children.Add(threadSliderTitle);
            formPanel.Children.Add(threadSlider);

            // Put stack panel in a scroll viewer
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = formPanel;
            
            // Add to dialog
            dialog.Content = scrollViewer;

            var result = await dialog.ShowAsync();

            // If the user presses save
            if (result == ContentDialogResult.Primary)
            {

                // Get inputs from the form
                string strGroupName = groupName.Text.ToString();
                string strSiteSelection = "";
                string strProductSelection = "";
                string strProxyGroup = "";
                string strProfileGroup = "";
                int threadCount = 0;

                if (siteSelectionList.SelectedValue != null)
                {
                    strSiteSelection = siteSelectionList.SelectedItem.ToString();
                }

                strProductSelection = productSpecificationInput.Text.ToString();
                if (proxyGroupList.SelectedItem != null)
                {
                    strProxyGroup = proxyGroupList.SelectedItem.ToString();
                }

                if (profileGroupList.SelectedItem != null)
                {
                    strProfileGroup = profileGroupList.SelectedItem.ToString();
                }

                threadCount = (int)threadSlider.Value;

                // Validate the form

                bool successfulData = true;
                if (
                        strGroupName.Length == 0 ||
                        strSiteSelection.Length == 0 ||
                        strProductSelection.Length == 0 ||
                        strProxyGroup.Length == 0 ||
                        strProfileGroup.Length == 0 
                    )
                {
                    successfulData = false;
                }

                if (!successfulData)
                {
                    NotifyUser(false, "Create Task Group", "Invalid data or missing fields");
                    return;
                }else
                {

                    int supportedSiteEnumIndex = Array.IndexOf(Enum.GetNames(typeof(SupportedSitesEnum)), strSiteSelection);

                    TaskGroup taskGroup = new TaskGroup();
                    taskGroup.groupName = strGroupName;
                    taskGroup.numThreads = threadCount;
                    taskGroup.productLink = strProductSelection;
                    taskGroup.chosenSite = (SupportedSitesEnum)supportedSiteEnumIndex;
                    
                    // Get proxy group
                    foreach (ProxyGroup proxyGroup in savedProxyGroups)
                    {
                        if (proxyGroup.GroupName == strProxyGroup)
                        {
                            taskGroup.proxyGroup = proxyGroup;
                            break;
                        }
                    }

                    // Get profile group
                    foreach (ProfileGroup profileGroup in savedProfileGroups)
                    {
                        if (profileGroup.GroupName == strProfileGroup)
                        {
                            taskGroup.profileGroup = profileGroup;
                            break;
                        }
                    }

                    // Get sizes
                    bool hasCustomSizes = false;
                    Collection<string> chosenSizes = new Collection<string>();
                    if (sizeSelectionExpander.Header == "Custom")
                    {
                        foreach (StackPanel sizeGroup in sizeSelectionList.Items)
                        {
                            foreach(CheckBox sizeSelection in sizeGroup.Children)
                            {
                                if ((bool)sizeSelection.IsChecked)
                                {
                                    hasCustomSizes = true;
                                    chosenSizes.Add(sizeSelection.Content.ToString());
                                }
                            }
                        }
                    }

                    if (hasCustomSizes)
                    {
                        taskGroup.chosenSizes = chosenSizes;
                    }else
                    {
                        taskGroup.chosenSizes = new Collection<string>();
                        foreach(string shoeSize in ShoeSizes)
                        {
                            taskGroup.chosenSizes.Add(shoeSize);
                        }
                    }

                    this.TaskView.AddGroup(taskGroup);
                    NotifyUser(true, "Create Task Group", "Succesfully created task group");
                    return;
                }

            }


        }

        private void AppBarButton_Remove_Click(object sender, RoutedEventArgs e)
        {

            // TODO add task logic communication to terminate tasks upon deletion

            if (TaskGroupList.SelectedIndex > -1)
            {
                this.TaskView.RemoveGroup((TaskGroupList.SelectedItem as TaskGroup));
                NotifyUser(true, "Delete task group", "Successfully deleted task group");
            }
           
            // Only on successful deletion.
            EmptyTaskStack.Visibility = Visibility.Visible;
            ItemsGridView.Visibility = Visibility.Collapsed;
            GroupActionControl.Visibility = Visibility.Collapsed;
        }

        private void ItemsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)ItemsGridView.ItemsPanelRoot;
            panel.ItemWidth = e.NewSize.Width;
        }

        private void TaskGroupList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (EmptyTaskStack.Visibility == Visibility.Visible)
            {
                EmptyTaskStack.Visibility = Visibility.Collapsed;
                GroupActionControl.Visibility = Visibility.Visible;
                ItemsGridView.Visibility = Visibility.Visible;
            }

            this.TaskView.SelectGroup((e.ClickedItem as TaskGroup).groupName);
            UpdateTaskState();
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

        private void UpdateTaskState()
        {
            StartTaskGroupBtn.IsEnabled = !this.TaskView.SelectedGroup.groupState.isGroupActive;
            StopTaskGroupBtn.IsEnabled = this.TaskView.SelectedGroup.groupState.isGroupActive;
        }

        private void StartTaskGroup_Click(object sender, RoutedEventArgs e)
        {
            TaskGroup taskGroup = (TaskGroupList.SelectedItem as TaskGroup);
            this.TaskView.StartTaskGroup(taskGroup);
            UpdateTaskState();
        }

        private void StopTaskGroup_Click(object sender, RoutedEventArgs e)
        {
            TaskGroup taskGroup = (TaskGroupList.SelectedItem as TaskGroup);
            this.TaskView.StopTaskGroup(taskGroup);
            UpdateTaskState();
        }
    }
}
