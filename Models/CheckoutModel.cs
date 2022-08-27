using Bermuda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{
    public class CheckoutModel : BindableBase
    {
        public string ProductSite { get; set; }
        public string SKU { get; set; }
        public string ProfileName { get; set; }
        public string Size { get; set; }
        public string ProductImage { get; set; }
    }
}
