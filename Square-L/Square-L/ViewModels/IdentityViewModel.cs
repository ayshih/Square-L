using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Square_L
{
    public class IdentityViewModel : INotifyPropertyChanged
    {
        public Identity identity;

        /// <summary>
        /// The nickname for an identity
        /// </summary>
        /// <returns></returns>
        public string Nickname
        {
            get
            {
                return identity.nickname;
            }
            set
            {
                if (value != identity.nickname)
                {
                    identity.nickname = value;
                    NotifyPropertyChanged("Nickname");
                }
            }
        }

        /// <summary>
        /// The last time an identity was used as a DateTime object
        /// </summary>
        /// <returns></returns>
        public DateTime LastUsed
        {
            get { return identity.lastUsed; }
            set
            {
                if (value != identity.lastUsed)
                {
                    identity.lastUsed = value;
                    NotifyPropertyChanged("LastUsedString");
                }
            }
        }

        /// <summary>
        /// The last time an identity was used as a string, prepended with "Last used: "
        /// </summary>
        /// <returns></returns>
        public string LastUsedString
        {
            get { return "Last used: " + LastUsed.ToString(); }
        }

        public byte[] masterKey { get { return identity.masterKey; } }
        public byte[] passwordSalt { get { return identity.passwordSalt; } }
        public byte[] passwordHash { get { return identity.passwordHash; } }

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