using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{
    public class EventContainer
    {
        public string eventType { get; set; }
        public string eventDataJson { get; set; }
    }

    public class TaskUpdate : EventArgs
    {
        public string TaskGroupId { get; set; }
        public string TaskThreadId { get; set; }
        public string StatusUpdate { get; set; }
        public string StatusColor { get; set; }
    }

    public class CheckoutUpdate : EventArgs
    {
        public string TaskGroupId { get; set; }
        public string ProfileName { get; set; }
        public string ProductSite { get; set; }
        public string ProductSize { get; set; }
        public string ProductSKU { get; set; }
        public string ProductImage { get; set; }
    }

}
