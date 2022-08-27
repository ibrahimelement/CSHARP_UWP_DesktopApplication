using Bermuda.Interfaces;
using System.Collections.ObjectModel;
using Bermuda.Models;
using System.Text.Json;
using System.Diagnostics;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Microsoft.UI.Xaml.CustomAttributes;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Bermuda.ViewModels
{

    public class TaskViewModel : BindableBase
    {

        public ObservableCollection<TaskGroup> TaskGroups;
        private EventHandler<TaskUpdate> taskEventHandler;
        private TaskGroup _selectedGroup;
        public TaskGroup SelectedGroup
        {
            get
            {
                return _selectedGroup;
            }
            set
            {
                _selectedGroup = value;
                OnPropertyChanged();
            }
        }

        public TaskViewModel(IDataService dataService, INavigationService navigationService, IBackendService backendService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _backendService = backendService;
            _navigationService.SetCleanupRoutine(this.ManualClean);
            this.LoadConfiguration();
            this._SubscribeToTaskUpdates();
        }
        public bool hasConfiguredSettings = false;

        public bool ManualClean()
        {
            _dataService.UpdateTaskGroupState(TaskGroups);
            TaskGroups = null;
            this._selectedGroup = null;
            this.SelectedGroup = null;
            this._UnsubscribeToTaskUpdates();
            GC.Collect();
            return true;
        }

        public void NavigateSettings()
        {
            _navigationService.NavigateTo("Settings");
        }

        /*
         * Read initial data from TaskView
         */
        bool LoadConfiguration()
        {

            TaskGroups = new ObservableCollection<TaskGroup>();
            SelectedGroup = new TaskGroup();
            SelectedGroup.taskList = new ObservableCollection<Task>();

            Collection<TaskGroup> loadedTaskGroupes = this._dataService.LoadTaskGroups();

            foreach (TaskGroup taskGroup in loadedTaskGroupes)
            {

                // Create a saved state
                if (taskGroup.groupState == null)
                {
                    TaskGroupState state = new TaskGroupState
                    {
                        groupName = taskGroup.groupName,
                        isGroupActive = false,
                        threadActivationList = new Collection<bool>()
                    };
                    taskGroup.groupState = state;
                }

                CreateTaskEntries(taskGroup);
                TaskGroups.Add(taskGroup);

            }

            SettingsModel settings = _dataService.GetSettings();
            this.hasConfiguredSettings = settings.isConfigured;

            return true;
        }


        public bool SelectGroup(string groupName)
        {

            for (int x = 0; x < TaskGroups.Count; x++)
            {
                if (TaskGroups[x].groupName == groupName)
                {
                    SelectedGroup.taskList.Clear();
                    SelectedGroup.groupName = groupName;
                    for (int i = 0; i < TaskGroups[x].taskList.Count; i++)
                    {
                        SelectedGroup.taskList.Add(TaskGroups[x].taskList[i]);
                    }
                    SelectedGroup.groupState = TaskGroups[x].groupState;
                    SelectedGroup.chosenSite = TaskGroups[x].chosenSite;
                    return true;
                }
            }

            return false;
        }

        /*
         * In charge of populating a task group with group provided data
         * Updating the task will be managed by the task handler service (future)
         */
        private bool CreateTaskEntries(TaskGroup taskGroup)
        {
            taskGroup.taskList = new ObservableCollection<Task>();

            string strChosenSite = Enum.GetNames(typeof(Enums.SupportedSitesEnum))[(int)taskGroup.chosenSite];

            for (int x = 0; x < taskGroup.numThreads; x++)
            {
                taskGroup.taskList.Add(new Task {
                    chosenSite = taskGroup.chosenSite,
                    strChosenSite = strChosenSite,
                    productLink = taskGroup.productLink,
                    Status = "Stopped",
                    StatusColor = new SolidColorBrush(Colors.White)
                });
            }
            return true;
        }

        public bool AddGroup(TaskGroup taskGroup)
        {

            TaskGroupState groupState = new TaskGroupState
            {
                groupName = taskGroup.groupName,
                isGroupActive = false,
                threadActivationList = new Collection<bool>()
            };
            taskGroup.groupState = groupState;
          
            this.CreateTaskEntries(taskGroup);
            this._dataService.SaveTaskGroups(taskGroup);
            this.TaskGroups.Add(taskGroup);
            return true;
        }

        public bool RemoveGroup(TaskGroup taskGroup)
        {
            this._dataService.RemoveTaskGroup(taskGroup);
            this.TaskGroups.Remove(taskGroup);
            return true;
        }

        // Used for task group creation
        public Collection<ProfileGroup> GetProfileGroups()
        {
            return this._dataService.LoadProfileGroups();
        }

        public Collection<ProxyGroup> GetProxyGroups()
        {
            return this._dataService.LoadProxyGroups();
        }

        public void StartTaskGroup(TaskGroup taskGroup)
        {
            this._SendSettings(taskGroup);
            this.SelectedGroup.groupState.isGroupActive = true;
            this.TaskGroups.First(gItem => gItem.groupName == taskGroup.groupName).groupState.isGroupActive = true;
            string strTaskGroup = JsonSerializer.Serialize<TaskGroup>(taskGroup);
            this._backendService.StartTaskGroup(strTaskGroup);
        }

        private void _SendSettings(TaskGroup taskGroup)
        {
            SettingsModel settings = new SettingsModel();
            settings = _dataService.GetSettings();
            string strJson = JsonSerializer.Serialize<SettingsModel>(settings);
            _backendService.ConfigureSettings(strJson);
            this._NotifySettings(taskGroup);
        }

        private void _NotifySettings(TaskGroup taskGroup)
        {

            for (int x = 0; x < taskGroup.numThreads; x++)
            {
                TaskUpdate taskUpdate = new TaskUpdate();
                taskUpdate.StatusColor = "Yellow";
                taskUpdate.StatusUpdate = "Launching";
                taskUpdate.TaskGroupId = taskGroup.groupName;
                taskUpdate.TaskThreadId = (x).ToString();
                this._ApplyTaskEvent(taskUpdate);
            }

        }

        public void StopTaskGroup(TaskGroup taskGroup)
        {
            this.SelectedGroup.groupState.isGroupActive = false;
            this.TaskGroups.First(gItem => gItem.groupName == taskGroup.groupName).groupState.isGroupActive = false;
            string strTaskGroup = JsonSerializer.Serialize<TaskGroup>(taskGroup);
            this._backendService.StopTaskGroup(strTaskGroup);
            // TODO call stop task group
        }

        private SolidColorBrush _GetProperBrush(string color)
        {
            switch(color)
            {
                case "Green":
                    return new SolidColorBrush(Colors.Lime);
                break;
                case "Red":
                    return new SolidColorBrush(Colors.Red);
                break;
                case "Yellow":
                    return new SolidColorBrush(Colors.Yellow);
                break;
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        private void _ApplyTaskEvent(TaskUpdate update)
        {
            
            
            // https://stackoverflow.com/questions/19341591/the-application-called-an-interface-that-was-marshalled-for-a-different-thread
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    var taskGroup = TaskGroups.FirstOrDefault(group => group.groupName == update.TaskGroupId);
                    if (taskGroup != null)
                    {
                        taskGroup.taskList[Int32.Parse(update.TaskThreadId)].Status = update.StatusUpdate;
                        taskGroup.taskList[Int32.Parse(update.TaskThreadId)].StatusColor = _GetProperBrush(update.StatusColor);//update.StatusColor;
                    }
                }
                catch(Exception err)
                {
                    Debug.WriteLine("Dispatch exception", err.Message);
                }
                    
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        
        
        }

        private void _SubscribeToTaskUpdates()
        {
            Random rng = new Random();
            int randomIdentifier = rng.Next(0, 100);
            this.taskEventHandler = (object Sender, TaskUpdate update) =>
            {
                this._ApplyTaskEvent(update);
            };
            this._backendService.ForwardTaskUpdates(this.taskEventHandler);
        }

        private void _UnsubscribeToTaskUpdates()
        {
            if (this.taskEventHandler != null)
            {
                this._backendService.UnsubscribeTaskUpdates(this.taskEventHandler);
            }
        }

        private void DummyPopulate()
        {
            for (int x = 0; x < 4; x++)
            {
                TaskGroup taskGroup = new TaskGroup();
                taskGroup.groupName = $"Group - {x}";
                taskGroup.numThreads = 10 + x;
                taskGroup.taskList = new ObservableCollection<Task>();

                for (int i = 0; i < taskGroup.numThreads; i++)
                {
                    Task task = new Task();
                    task.chosenSite = Enums.SupportedSitesEnum.FootlockerUS;
                    task.Status = "Default";
                    task.productLink = "53558034" + x;
                    taskGroup.taskList.Add(task);
                }

                TaskGroups.Add(taskGroup);
            }
        }

    }

}
