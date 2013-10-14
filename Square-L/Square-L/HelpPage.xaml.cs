using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Security.Cryptography;
using ZXing;
using Crypto;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Square_L
{
    public partial class HelpPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private Random _random;
        private CryptoRuntimeComponent _crypto;

        public HelpPage()
        {
            InitializeComponent();

            this.DataContext = this;

            _random = new Random();
            _crypto = new CryptoRuntimeComponent();

            LongText = "";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void SCryptTest_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Running scrypt tests";

                Dispatcher.BeginInvoke(() => TestSCrypt());
            }
        }

        public void TestSCrypt()
        {
            LongText = "";

            var password = System.Text.Encoding.UTF8.GetBytes("password");

            var passwordSalt = new byte[8];
            _random.NextBytes(passwordSalt);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _crypto.SCrypt(password, passwordSalt, 14, 8, 1);
            stopwatch.Stop();
            LongText += "Interactive verification:\n{2^14,8,1}: " + stopwatch.ElapsedMilliseconds.ToString() + " ms\n\n";
            LongText += "Import verification:\n{2^14,8,100}: ~" + (100*stopwatch.ElapsedMilliseconds/1000.0).ToString() + " s\n\n";

            LongText += "Other sets of parameters:\n";
            int[] list_log2_N = { 13, 14, 15 };
            int[] list_r = { 8 };
            int[] list_p = { 1, 4 };

            foreach (var log2_N in list_log2_N)
                foreach (var r in list_r)
                    foreach (var p in list_p)
                    {
                        stopwatch.Restart();
                        var scryptResult = _crypto.SCrypt(password, passwordSalt, log2_N, r, p);
                        var output = "{2^" + log2_N + "," + r + "," + p + "}: " + stopwatch.ElapsedMilliseconds.ToString() + " ms";
                        LongText += output + "\n";
                        Debug.WriteLine(output);
                    }

            stopwatch.Stop();
            SystemTray.ProgressIndicator.IsVisible = false;
        }

        private string _longText;
        public string LongText
        {
            get { return _longText; }
            set
            {
                if (value != _longText)
                {
                    _longText = value;
                    NotifyPropertyChanged("LongText");
                }
            }
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