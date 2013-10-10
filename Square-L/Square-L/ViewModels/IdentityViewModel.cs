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
        private string _nickname;
        /// <summary>
        /// The nickname for an identity
        /// </summary>
        /// <returns></returns>
        public string Nickname
        {
            get
            {
                return _nickname;
            }
            set
            {
                if (value != _nickname)
                {
                    _nickname = value;
                    NotifyPropertyChanged("Nickname");
                }
            }
        }

        private DateTime _lastUsed;
        public DateTime LastUsed
        {
            get { return _lastUsed; }
            set
            {
                if (value != _lastUsed)
                {
                    _lastUsed = value;
                    NotifyPropertyChanged("LastUsed");
                }
            }
        }

        public string LastUsedString
        {
            get { return "Last used: " + _lastUsed.ToString(); }
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