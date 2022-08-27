using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Models
{

    public class Billing
    {
        // Billing information
        public string cardNumber { get; set; }
        public string cardExpMonth { get; set; }
        public string cardExpYear { get; set; }
        public string cardCVV { get; set; }
        public string billingState { get; set; }
        public string billingStateCode { get; set; }
        public string billingCity { get; set; }
        public string billingAddress1 { get; set; }
        public string billingAddress2 { get; set; }
        public string billingZip { get; set; }
    }

    public class Delivery
    {
        // Locational information (Delivery)
        public string deliveryState { get; set; }
        public string deliveryStateCode { get; set; }
        public string deliveryCity { get; set; }
        public string deliveryAddress1 { get; set; }
        public string deliveryAddress2 { get; set; }
        public string deliveryZip { get; set; }
    }

    public class Digital
    {
        // Digital credentials
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class Personal
    {
        public string profileName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    // profile name, first name, last name, address 1, address 2, city, state, zip, card, expM, expY, cvv, email
    public class Profile
    {
        public Personal personalInfo { get; set; }
        public Billing billingInfo { get; set; }
        public Delivery deliveryInfo { get; set; }
        public Digital digitalInfo { get; set; }
    }

    public class ProfileGroup
    {
        public string GroupName { get; set; }
        public ObservableCollection<Profile> profiles { get; set; }
    }

}
