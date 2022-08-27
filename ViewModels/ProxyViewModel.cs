using Bermuda.Enums;
using Bermuda.Interfaces;
using Bermuda.Models;
using Bermuda.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Bermuda.ViewModels
{
    public class ProxyViewModel : BindableBase
    {

        public ObservableCollection<ProxyGroup> ProxyGroups;
        public ProxyGroup SelectedProxyGroup;
        private string _selectedGroup;

        public ProxyViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _navigationService.SetCleanupRoutine(this.ManualClean);
            LoadConfiguration();
        }

        ~ProxyViewModel()
        {

        }

        // Todo make interface to use DataService
        bool ValidateData()
        {
            return true;
        }

        public bool ManualClean()
        {
            this._selectedGroup = null;
            this.ProxyGroups = null;
            this._selectedGroup = null;
            GC.Collect();
            return true;
        }
        
        /*
         * Will read stored data from the dataservice
         */
        bool LoadConfiguration()
        {
            // Load initial data here
            ProxyGroups = new ObservableCollection<ProxyGroup>();
            this.SelectedProxyGroup = new ProxyGroup();
            this.SelectedProxyGroup.Proxies = new ObservableCollection<Proxy>();

            Collection<ProxyGroup> importedProxyGroups = this._dataService.LoadProxyGroups();

            foreach (ProxyGroup proxyGroup in importedProxyGroups)
            {
                ProxyGroups.Add(proxyGroup);
            }
           
            return true;
        }
        
        /*
         * Will update the local observable collections of proxies
         */
        public bool SelectGroup(string groupName)
        {

            for (int x = 0; x < this.ProxyGroups.Count; x++)
            {
                if (this.ProxyGroups[x].GroupName == groupName)
                {

                    _selectedGroup = groupName;
                    SelectedProxyGroup.Proxies.Clear();
                    
                    for (int y = 0; y < this.ProxyGroups[x].Proxies.Count; y++)
                    {
                        SelectedProxyGroup.Proxies.Add(this.ProxyGroups[x].Proxies[y]);
                    }
                    
                }
            }

            return false;
        }

        /*
         *  Will add a group to the local proxy list and to the dataservice
         */
        public int AddGroup(string groupName, string groupValue)
        {
            
            ProxyGroup proxyGroup = new ProxyGroup();
            proxyGroup.Proxies = new ObservableCollection<Proxy>();
            proxyGroup.GroupName = groupName;
           
            string[] proxies = groupValue.Split("\r");
            var proxyImportEnumCount = Enum.GetNames(typeof(ProxyEnum)).Length;

            for (int x = 0; x < proxies.Length; x++)
            {
                string[] proxyEntryFields = proxies[x].Split(':');

                if (proxyEntryFields.Length != proxyImportEnumCount)
                {
                    continue;
                }
                
                Proxy proxy = new Proxy{
                    host = proxyEntryFields[(int)ProxyEnum.host],
                    port = proxyEntryFields[(int)ProxyEnum.port],
                    username = proxyEntryFields[(int)ProxyEnum.username],
                    password = proxyEntryFields[(int)ProxyEnum.password]
                };

                proxyGroup.Proxies.Add(proxy);
            }

            if (proxyGroup.Proxies.Count > 0)
            {
                _dataService.SaveProxyGroup(proxyGroup);
                ProxyGroups.Add(proxyGroup);
                this.SelectGroup(proxyGroup.GroupName);
                return proxyGroup.Proxies.Count;
            }
            else
            {
                return 0;
            }

        }

        /*
         * Will remove a group from the local list, and the from the dataservices
         */
        public bool RemoveGroup(string groupName)
        {

            bool successfulDeletion = false;
            for (int x = 0; x < this.ProxyGroups.Count && !successfulDeletion; x++)
            {
                if (this.ProxyGroups[x].GroupName == groupName)
                {
                    this._dataService.RemoveProxyGroup(this.ProxyGroups[x]);
                    this.ProxyGroups.RemoveAt(x);
                    this.SelectedProxyGroup.Proxies.Clear();
                    successfulDeletion = true;
                }
            }

            return successfulDeletion;
        }

        private bool DummyPopulate()
        {

            for (int x = 0; x < 3; x++)
            {
                ProxyGroup group = new ProxyGroup();
                group.Proxies = new ObservableCollection<Proxy>();
                group.GroupName = $"Group - {x}";

                for (int i = 0; i < 20; i++)
                {
                    Proxy proxy = new Proxy
                    {
                        host = "http://proxies.boroinc.com",
                        port = $"{1000 * x + i}",
                        username = "admin",
                        password = "password"
                    };

                    group.Proxies.Add(proxy);

                }
                ProxyGroups.Add(group);
                this._dataService.SaveProxyGroup(group);
            }

            if (ProxyGroups.Count > 0)
            {
                this.SelectGroup(ProxyGroups[0].GroupName);
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
