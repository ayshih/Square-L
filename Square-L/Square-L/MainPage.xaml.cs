using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace Square_L
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void IdentitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox selector = sender as ListBox;
            if (selector == null) return;

            IdentityViewModel identity = selector.SelectedItem as IdentityViewModel;
            if (identity == null) return;

            NavigationService.Navigate(
                new Uri("/ScanPage.xaml?selectedIndex="
                    + selector.SelectedIndex,
                    UriKind.Relative));

            selector.SelectedItem = null;
        }

        private void ContextMenuRename_Click(object sender, RoutedEventArgs e)
        {
            IdentityViewModel identity = (sender as MenuItem).DataContext as IdentityViewModel;

            MessageBox.Show("Renaming not yet implemented", identity.Nickname, MessageBoxButton.OK);
        }

        private void ContextMenuChangePassword_Click(object sender, RoutedEventArgs e)
        {
            IdentityViewModel identity = (sender as MenuItem).DataContext as IdentityViewModel;

            MessageBox.Show("Password changing not yet implemented", identity.Nickname, MessageBoxButton.OK);
        }

        private void ContextMenuExport_Click(object sender, RoutedEventArgs e)
        {
            var identity = (sender as MenuItem).DataContext as IdentityViewModel;
            if (identity == null) return;

            var index = App.ViewModel.Identities.IndexOf(identity);

            NavigationService.Navigate(
                new Uri("/ExportIdentityPage.xaml?selectedIndex="
                    + index,
                    UriKind.Relative));
        }

        private void ContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            IdentityViewModel identity = (sender as MenuItem).DataContext as IdentityViewModel;

            MessageBoxResult m = MessageBox.Show("Are you sure you want to delete this identity?", identity.Nickname, MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                App.ViewModel.Identities.Remove(identity);

                var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                settings.Remove("identity_"+identity.Nickname);
                settings.Save();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarCreateIdentity_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateIdentityPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarHelp_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
        }

    }
}