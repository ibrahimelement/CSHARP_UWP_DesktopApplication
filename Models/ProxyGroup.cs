using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{

    public class Proxy
    {
        public string username { get; set; }
        public string password { get; set; }
        public string host { get; set; }
        public string port { get; set; }
    }

    public class ProxyGroup
    {
        public string GroupName { get; set; }
        public ObservableCollection<Proxy> Proxies { get; set; }
    }
}
