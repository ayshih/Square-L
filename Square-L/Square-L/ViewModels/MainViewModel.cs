using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace Square_L
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Identities = new ObservableCollection<IdentityViewModel>();
        }

        /// <summary>
        /// A collection for IdentityViewModel objects.
        /// </summary>
        public ObservableCollection<IdentityViewModel> Identities { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        public void LoadData()
        {
            var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;

            if (!settings.Contains("identity_test identity")) {
                var identity = new Identity() { nickname = "test identity", lastUsed = new DateTime(2013, 10, 7, 17, 47, 0), masterKey = Convert.FromBase64String("VxXA0VcczUN6nj/9bMVlCeP7ogpqhmLCK54GIFTSl1s="), passwordSalt = Convert.FromBase64String("Ze6tha++1E0="), passwordHash = Convert.FromBase64String("TlA6rTzAcCYWm8o/UF6sk3i8mU2JR/db34/6nE3HKDg=") };
                settings.Add("identity_" + identity.nickname, identity);
                settings.Save();
            }

            foreach (var key in settings.Keys)
            {
                if (key.ToString().StartsWith("identity_"))
                {
                    this.Identities.Add(new IdentityViewModel() { identity = (Identity)settings[key.ToString()] });
                }
            }

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}