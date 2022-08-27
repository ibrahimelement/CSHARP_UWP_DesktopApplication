using Bermuda.Enums;
using Bermuda.Interfaces;
using Bermuda.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;
using System.Text.Json;
using System.IO;
using Windows.Storage;

namespace Bermuda.ViewModels
{
    public class ProfileViewModel : BindableBase
    {

        public ObservableCollection<ProfileGroup> ProfileGroups;
        public ProfileGroup SelectedProfileGroup;

        public ProfileViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _navigationService.SetCleanupRoutine(this.ManualCleanup);
            LoadConfiguration();
        }

        public bool ManualCleanup()
        {
            this.ProfileGroups = null;
            this.SelectedProfileGroup = null;
            GC.Collect();
            return true;
        }

        /*
         * Read data from the data service
         */
        bool LoadConfiguration()
        {
            ProfileGroups = new ObservableCollection<ProfileGroup>();
            SelectedProfileGroup = new ProfileGroup();
            SelectedProfileGroup.profiles = new ObservableCollection<Profile>();
            Collection<ProfileGroup> importedProfiles = this._dataService.LoadProfileGroups();

            foreach(ProfileGroup profileGroup in importedProfiles)
            {
                ProfileGroups.Add(profileGroup);
            }
            
            return true;
        }

        public bool SelectGroup(string groupName)
        {

            for (int x = 0; x < ProfileGroups.Count; x++)
            {
                if (ProfileGroups[x].GroupName == groupName)
                {

                    SelectedProfileGroup.profiles.Clear();
                    for (int i = 0; i < ProfileGroups[x].profiles.Count; i++)
                    {
                        SelectedProfileGroup.profiles.Add(
                            ProfileGroups[x].profiles[i]
                        );
                    }
                    return true;
                }
            }

            return false;
        }

        public bool RemoveGroup(ProfileGroup profileGroup)
        {
            this._dataService.RemoveProfileGroup(profileGroup);
            this.SelectedProfileGroup.profiles.Clear();
            this.ProfileGroups.Remove(profileGroup);
            return true;
        }

        public async Task<int> ImportProfiles(string groupName, string profileDump)
        {

            string[] fieldDelimOptions = { ", ", "," };
            string[] profileEntries = profileDump.Split(Environment.NewLine.ToCharArray());

            // Used only for reference and field count
            string[] importFormat =
            {
                "profileName", "firstName", "lastName", "address1", "address2", "city", "state", "stateCode", "zip", "card", "expM", "expY", "cvv", "email", "phone"
            };


            if (profileEntries.Length > 0)
            {
                
                string chosenDelim = "";
                bool foundDelim = false;
                foreach(string fieldDelim in fieldDelimOptions)
                {
                    if (profileEntries[0].IndexOf(fieldDelim) > -1)
                    {
                        chosenDelim = fieldDelim;
                        foundDelim = true;
                        break;
                    }
                }

                if (foundDelim)
                {

                    ProfileGroup profileGroup = new ProfileGroup();
                    profileGroup.profiles = new ObservableCollection<Profile>();
                    profileGroup.GroupName = groupName;

                    for (int x = 0; x < profileEntries.Length; x++)
                    {
                    
                        string currentEntry = profileEntries[x];
                        string[] fields = currentEntry.Split(chosenDelim);

                        if (fields.Length != importFormat.Length)
                        {
                            return 0;
                        }

                        // Create easy dictionary collection based on the profile mapping enumeration

                        Dictionary<ProfileEnum, string> profileEntryMapping = new Dictionary<ProfileEnum, string>();
                        for (int i = 0; i < importFormat.Length; i++)
                        {
                            profileEntryMapping.Add((ProfileEnum)i, fields[i]);
                        }

                        Profile profile = new Profile();
                        profile.billingInfo = new Billing();
                        profile.deliveryInfo = new Delivery();
                        profile.digitalInfo = new Digital();
                        profile.personalInfo = new Personal();

                        // Personal
                        profile.personalInfo.profileName = profileEntryMapping[ProfileEnum.ProfileName];
                        profile.personalInfo.firstName = profileEntryMapping[ProfileEnum.FirstName];
                        profile.personalInfo.lastName = profileEntryMapping[ProfileEnum.LastName];

                        // Digital
                        profile.digitalInfo.email = profileEntryMapping[ProfileEnum.Email];
                        profile.digitalInfo.phone = profileEntryMapping[ProfileEnum.Phone];

                        // Delivery
                        profile.deliveryInfo.deliveryAddress1 = profileEntryMapping[ProfileEnum.Address1];
                        profile.deliveryInfo.deliveryAddress2 = profileEntryMapping[ProfileEnum.Address2];
                        profile.deliveryInfo.deliveryCity = profileEntryMapping[ProfileEnum.City];
                        profile.deliveryInfo.deliveryState = profileEntryMapping[ProfileEnum.State];
                        profile.deliveryInfo.deliveryStateCode = profileEntryMapping[ProfileEnum.StateCode];
                        profile.deliveryInfo.deliveryZip = profileEntryMapping[ProfileEnum.Zip];

                        // Billing
                        profile.billingInfo.billingAddress1 = profileEntryMapping[ProfileEnum.Address1];
                        profile.billingInfo.billingAddress2 = profileEntryMapping[ProfileEnum.Address2];
                        profile.billingInfo.billingCity = profileEntryMapping[ProfileEnum.City];
                        profile.billingInfo.billingState = profileEntryMapping[ProfileEnum.State];
                        profile.billingInfo.billingStateCode = profileEntryMapping[ProfileEnum.StateCode];
                        profile.billingInfo.billingZip = profileEntryMapping[ProfileEnum.Zip];
                        profile.billingInfo.cardExpMonth = profileEntryMapping[ProfileEnum.ExpM];
                        profile.billingInfo.cardExpYear = profileEntryMapping[ProfileEnum.ExpY];
                        profile.billingInfo.cardCVV = profileEntryMapping[ProfileEnum.CVV];
                        profile.billingInfo.cardNumber = profileEntryMapping[ProfileEnum.Card];

                        profileGroup.profiles.Add(profile);

                    }

                    // Then I need to create a local version of it here to avoid multiple imports, the data service should only be imported one time

                    this._dataService.SaveProfileGroup(profileGroup);
                    this.ProfileGroups.Add(profileGroup);

                    return profileEntries.Length;
                }

            }

            return 0;
        }

        private void DummyPopulate()
        {
            /*
           for (int x = 0; x < 3; x++)
           {

               ProfileGroup profileGroup = new ProfileGroup();
               profileGroup.profiles = new ObservableCollection<Profile>();
               profileGroup.GroupName = $"Group - {x}";

               string[] Names = { "Alex", "John", "King", "Ahmed" };
               string[] Addresses = { "Bottom Ave", "Right Cir", "Newport Drive", "Kingsway" };
               string[] States = { "California", "Texas", "Gerogia", "New Virginia" };

               var rand = new Random();
               for (int i = 0; i < 10; i++)
               {

                   Profile profile = new Profile();
                   profile.billingInfo = new Billing();
                   profile.deliveryInfo = new Delivery();
                   profile.personalInfo = new Personal();
                   profile.digitalInfo = new Digital();

                   // Random data generation
                   string randomAddress = $"{rand.Next(1000, 9000)} {Addresses[rand.Next(0, Addresses.Length)]}";
                   string randomFName = Names[rand.Next(0, Names.Length)];
                   string randomLName = Names[rand.Next(0, Names.Length)];
                   string randomUsername = randomLName + $"{rand.Next(1000, 9000)}";
                   string randomState = $"{States[rand.Next(0, States.Length)]}";
                   string randomCity = $"ExampleCity-{i}";
                   string randomZip = $"{rand.Next(10000, 90000)}";
                   string randomEmail = $"{randomUsername}@gmail.com";

                   // Billing
                   profile.billingInfo.billingAddress1 = randomAddress;
                   profile.billingInfo.billingCity = randomCity;
                   profile.billingInfo.billingState = randomState;
                   profile.billingInfo.billingZip = randomZip;
                   profile.billingInfo.cardExpMonth = $"{rand.Next(0, 12)}";
                   profile.billingInfo.cardExpYear = $"{rand.Next(2021, 2025)}";
                   profile.billingInfo.cardNumber = $"***********{rand.Next(1000, 9999)}";
                   profile.billingInfo.billingAddress2 = "Empty";

                   // Digital
                   profile.digitalInfo.email = randomEmail;
                   profile.digitalInfo.password = "*******";
                   profile.digitalInfo.username = randomUsername;

                   // Delivery
                   profile.deliveryInfo.deliveryAddress1 = randomAddress;
                   profile.deliveryInfo.deliveryAddress2 = "Empty";
                   profile.deliveryInfo.deliveryCity = randomCity;
                   profile.deliveryInfo.deliveryState = randomState;
                   profile.deliveryInfo.deliveryZip = randomZip;

                   // Personal
                   profile.personalInfo.profileName = $"NikeProfile-{x+i}";
                   profile.personalInfo.firstName = randomFName;
                   profile.personalInfo.lastName = randomLName;

                   // Add to the list
                   profileGroup.profiles.Add(profile);

               }

               this._dataService.SaveProfileGroup(profileGroup);
               ProfileGroups.Add(profileGroup);
           }
           */
        }


    }
}
