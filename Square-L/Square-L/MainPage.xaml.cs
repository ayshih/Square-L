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
            
            //identity.Nickname += ".";

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
            IdentityViewModel identity = (sender as MenuItem).DataContext as IdentityViewModel;

            MessageBox.Show("Exporting not yet implemented", identity.Nickname, MessageBoxButton.OK);
        }

        private void ContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            IdentityViewModel identity = (sender as MenuItem).DataContext as IdentityViewModel;

            MessageBoxResult m = MessageBox.Show("Are you sure you want to delete this identity?", identity.Nickname, MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                App.ViewModel.Identities.Remove(identity);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var frame = (PhoneApplicationFrame)App.Current.RootVisual;
            if (frame.CanGoBack)
            {
                if (frame.BackStack.First().Source.ToString().StartsWith("/ScanPage.xaml"))
                {
                    NavigationService.RemoveBackEntry();
                    if (frame.CanGoBack)
                    {
                        if (frame.BackStack.First().Source.ToString().StartsWith("/MainPage.xaml"))
                        {
                            NavigationService.RemoveBackEntry();
                        }
                    }
                }
            }
        }

    }
}