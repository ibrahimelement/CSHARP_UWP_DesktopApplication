using Bermuda.Models;
using System;
using System.Collections.ObjectModel;

namespace Bermuda.Interfaces
{
    public interface IBackendService
    {

        bool Connect();
        bool Disconnect();

        // Task updates
        bool ForwardTaskUpdates(EventHandler<TaskUpdate> eventHandler);
        bool UnsubscribeTaskUpdates(EventHandler<TaskUpdate> eventHandler);

        // Checkout updates
        bool ForwardCheckoutUpdates(EventHandler<CheckoutUpdate> eventHandler);
        bool UnsubscribeCheckoutUpdates(EventHandler<CheckoutUpdate> eventHandler);
        Collection<TaskUpdate> GetSavedUpdates(TaskGroup taskGroup);

        // Settings
        bool ConfigureSettings(string jsonSettings);

        // Actions
        bool StartTaskGroup(string jsonTaskGroup);
        bool StopTaskGroup(string jsonTaskGroup);
        System.Threading.Tasks.Task<int> ValidateLicense(string licenseKey);

        // Updates
        System.Threading.Tasks.Task<bool> DownloadLatestUpdate(EventHandler<int> eventHandler);
        System.Threading.Tasks.Task<bool> CheckUpdate();

        // Backend status
        bool BackendOnlineEvent(EventHandler<bool> eventHandler);

    }
}
