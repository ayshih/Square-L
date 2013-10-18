using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Square_L
{
    public partial class ExportIdentityPage : PhoneApplicationPage
    {
        public ExportIdentityPage()
        {
            InitializeComponent();
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

                ExportMasterKey();
            }
        }

        private async void ExportMasterKey()
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            var identity = new Identity(((IdentityViewModel)DataContext).identity);

            try
            {
                SystemTray.ProgressIndicator.Text = "Verifying password";
                var scryptResult = await identity.GetSCryptResult(PasswordBox.Password);

                SystemTray.ProgressIndicator.Text = "Encrypting identity for export";
                var newParameters = new Crypto.SCryptParameters { log2_N = 14, r = 8, p = 100 };
                await identity.ChangeSCryptParameters(PasswordBox.Password, newParameters, scryptResult);

                Image.Source = identity.GetQRCode();
            }
            catch (Exception)
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);

                NavigationService.GoBack();
            }

            SystemTray.ProgressIndicator.IsVisible = false;
        }
    }
}