using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{
    public class UserDataModel
    {

        public string licenseKey { get; set; }
        public Collection<ProfileGroup> profileGroups { get; set; }
        public Collection<TaskGroup> taskGroups { get; set; }
        public Collection<ProxyGroup> proxyGroups { get; set; }
        public Collection<CheckoutModel> checkouts { get; set; }
        public SettingsModel settings { get; set; }
        public bool shareCheckouts { get; set; }
        public int numBrowsers { get; set; }
        public string currentBackendVersion { get; set; }
    }
}
