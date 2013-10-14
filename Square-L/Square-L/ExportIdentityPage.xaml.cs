using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Security.Cryptography;
using Crypto;
using ZXing;

namespace Square_L
{
    public partial class ExportIdentityPage : PhoneApplicationPage
    {
        private CryptoRuntimeComponent _crypto;
        private Random _random;

        public ExportIdentityPage()
        {
            InitializeComponent();

            _crypto = new CryptoRuntimeComponent();
            _random = new Random();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("selectedIndex", out selectedIndex))
            {
                int index = int.Parse(selectedIndex);
                DataContext = App.ViewModel.Identities[index];
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            UsePassword();
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                UsePassword();
            }
        }

        private void UsePassword()
        {
            if (PasswordBox.Password != "")
            {
                PasswordGrid.Visibility = System.Windows.Visibility.Collapsed;
                ImageGrid.Visibility = System.Windows.Visibility.Visible;

                SystemTray.ProgressIndicator.IsVisible = true;

                Dispatcher.BeginInvoke(() => ExportMasterKey());
            }
        }

        private void ExportMasterKey()
        {
            var password = System.Text.Encoding.UTF8.GetBytes(PasswordBox.Password);
            var passwordSalt = ((IdentityViewModel)DataContext).passwordSalt;
            var passwordHash = ((IdentityViewModel)DataContext).passwordHash;
            var masterKey = ((IdentityViewModel)DataContext).masterKey;

            Debug.WriteLine("Stored master key: " + Base64Url.Encode(masterKey));
            Debug.WriteLine("Password salt: " + Base64Url.Encode(passwordSalt));

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var scryptResult = _crypto.SCrypt(password, passwordSalt, 14, 8, 1);
            stopwatch.Stop();
            Debug.WriteLine("SCrypt of password+salt: " + Base64Url.Encode(scryptResult) + " (" + stopwatch.ElapsedMilliseconds.ToString() + " ms)");

            var _SHA256 = new SHA256Managed();

            var passwordCheck = _SHA256.ComputeHash(scryptResult);
            Debug.WriteLine("Password hash: " + Base64Url.Encode(passwordHash));
            Debug.WriteLine("Password check: " + Base64Url.Encode(passwordCheck));

            SystemTray.ProgressIndicator.IsVisible = false;

            if (Base64Url.Encode(passwordCheck).Equals(Base64Url.Encode(passwordHash)))
            {
                var trueMasterKey = Utility.Xor(masterKey, scryptResult);
                Debug.WriteLine("True master key: " + Base64Url.Encode(trueMasterKey));

                var newPasswordSalt = new byte[8];
                _random.NextBytes(newPasswordSalt);

                Debug.WriteLine("New password salt: " + Base64Url.Encode(newPasswordSalt));

                stopwatch.Restart();
                var newScryptResult = _crypto.SCrypt(password, newPasswordSalt, 14, 8, 100);
                stopwatch.Stop();
                Debug.WriteLine("SCrypt of password+new salt: " + Base64Url.Encode(newScryptResult) + " (" + stopwatch.ElapsedMilliseconds.ToString() + " ms)");

                var newPasswordHash = _SHA256.ComputeHash(newScryptResult);
                Debug.WriteLine("New password hash: " + Base64Url.Encode(newPasswordSalt));

                var newMasterKey = Utility.Xor(trueMasterKey, newScryptResult);
                Debug.WriteLine("New master key: " + Base64Url.Encode(newMasterKey));

                // The specification for the export format has not been decided.  Here it is:
                // byte  0: signature algorithm version
                //       1: encrypted master key (32 bytes)
                //      33: password algorithm version
                //      34: password salt (8 bytes)
                //      42: password hash (32 bytes)
                //      74: scrypt log_2(N) parameter
                //      75: scrypt r parameter
                //      76: scrypt p parameter (2 bytes)
                var export = new byte[78];
                export[0] = 1;
                Buffer.BlockCopy(newMasterKey, 0, export, 1, 32);
                export[33] = 1;
                Buffer.BlockCopy(newPasswordSalt, 0, export, 34, 8);
                Buffer.BlockCopy(newPasswordHash, 0, export, 42, 32);
                export[74] = 14;
                export[75] = 8;
                Buffer.BlockCopy(BitConverter.GetBytes((UInt16)100), 0, export, 76, 2);

                var options = new ZXing.QrCode.QrCodeEncodingOptions { Margin = 2, Width = 300, Height = 300 };
                var writer = new BarcodeWriter() { Format = BarcodeFormat.QR_CODE, Options = options };
                var qrcode = writer.Write(Base64Url.Encode(export));
                Image.Source = qrcode;
            }
            else
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);

                NavigationService.GoBack();
            }
        }
    }
}