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
        private SHA256Managed _SHA256;
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

            _SHA256 = new SHA256Managed();
            var hash = _SHA256.ComputeHash(BitConverter.GetBytes(DateTime.Now.Ticks));

            var writer = new BarcodeWriter() { Format = BarcodeFormat.QR_CODE };
            var qrcode = writer.Write(Base64Url.Encode(hash));
            Image.Source = qrcode;
        }

        public void TestSCrypt()
        {
            LongText = "";

            var password = System.Text.Encoding.UTF8.GetBytes("password");

            var passwordSalt = new byte[8];
            _random.NextBytes(passwordSalt);

            var scryptResult = new byte[32];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int[] list_log2_N = { 14, 16 };
            int[] list_r = { 8, 16 };
            int[] list_p = { 1, 4 };

            foreach (var log2_N in list_log2_N)
                foreach (var r in list_r)
                    foreach (var p in list_p)
                    {
                        _crypto.SCrypt(scryptResult, password, password.Length, passwordSalt, passwordSalt.Length, log2_N, r, p);
                        var output = "{2^" + log2_N + "," + r + "," + p + "}: " + stopwatch.ElapsedMilliseconds.ToString() + " ms";
                        LongText += output + "\n";
                        Debug.WriteLine(output);
                        stopwatch.Restart();
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

        private void SCryptTest_Click(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            Dispatcher.BeginInvoke(() => TestSCrypt());
        }
    }
}