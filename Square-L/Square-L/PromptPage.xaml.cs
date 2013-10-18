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
    public partial class PromptPage : PhoneApplicationPage
    {
        private string _mode;

        public PromptPage()
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

            if (NavigationContext.QueryString.TryGetValue("mode", out _mode))
            {
                switch (_mode)
                {
                    case "rename":
                        NewNickname.Visibility = System.Windows.Visibility.Visible;
                        NewPassword.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case "password":
                        NewNickname.Visibility = System.Windows.Visibility.Collapsed;
                        NewPassword.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UsePassword();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void UsePassword()
        {
            if (PasswordBox.Password != "")
            {
                switch (_mode)
                {
                    case "rename":
                        if (NewNicknameBox.Text == "") return;
                        RenameIdentity();
                        break;
                    case "password":
                        if (NewPassword1Box.Password != NewPassword2Box.Password)
                        {
                            NewPassword1Box.Password = "";
                            NewPassword2Box.Password = "";
                        }
                        if (NewPassword1Box.Password == "") return;
                        ChangePassword();
                        break;
                }
            }
        }

        private async void RenameIdentity()
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            var identity = ((IdentityViewModel)DataContext).identity;

            try
            {
                SystemTray.ProgressIndicator.Text = "Verifying password";
                var scryptResult = await identity.GetSCryptResult(PasswordBox.Password);

                var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                settings.Remove("identity_" + identity.nickname);

                ((IdentityViewModel)DataContext).Nickname = NewNicknameBox.Text;

                settings.Add("identity_" + identity.nickname, identity);
                settings.Save();
            }
            catch (Exception)
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);
            }

            SystemTray.ProgressIndicator.IsVisible = false;
            NavigationService.GoBack();
        }

        private async void ChangePassword()
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            var identity = ((IdentityViewModel)DataContext).identity;

            try
            {
                SystemTray.ProgressIndicator.Text = "Verifying password";
                var scryptResult = await identity.GetSCryptResult(PasswordBox.Password);

                SystemTray.ProgressIndicator.Text = "Changing password";
                await identity.ChangeSCryptParameters(NewPassword1Box.Password, identity.parameters, scryptResult);

                var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                settings.Remove("identity_" + identity.nickname);
                settings.Add("identity_" + identity.nickname, identity);
                settings.Save();
            }
            catch (Exception)
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);
            }

            SystemTray.ProgressIndicator.IsVisible = false;
            NavigationService.GoBack();
        }
    }
}