using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{
    public class SettingsModel
    {
        public bool isConfigured { get; set; }
        public ProxyGroup browserProxies { get; set; }
        public int numBrowser { get; set; }
        public string twoCaptchaApiKey { get; set; }
        public bool shareCheckouts { get; set; }
    }
}
