﻿using System;
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

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            TestSCrypt();
        }

        public async void TestSCrypt()
        {
            SystemTray.ProgressIndicator.Text = "Running scrypt test";

            LongText = "";

            var password = System.Text.Encoding.UTF8.GetBytes("password");

            var passwordSalt = new byte[8];
            _random.NextBytes(passwordSalt);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var parameters = new SCryptParameters { log2_N = 14, r = 8, p = 1 };
            await _crypto.SCryptAsync(password, passwordSalt, parameters);
            stopwatch.Stop();
            LongText += "Interactive verification:\n{2^14,8,1}: " + stopwatch.ElapsedMilliseconds.ToString() + " ms\n\n";
            LongText += "Import verification:\n{2^14,8,100}: ~" + (100*stopwatch.ElapsedMilliseconds/1000.0).ToString() + " s\n\n";

            LongText += "Other sets of parameters:\n";
            int[] list_log2_N = { 13, 14, 15 };
            int[] list_r = { 8, 16 };
            int[] list_p = { 1, 4 };

            int count = 0;
            int total = list_log2_N.Length * list_r.Length * list_p.Length;
            foreach (var log2_N in list_log2_N)
                foreach (var r in list_r)
                    foreach (var p in list_p)
                    {
                        SystemTray.ProgressIndicator.Text = "Running additional scrypt test " + (++count) + " of " + total;

                        parameters = new SCryptParameters { log2_N = log2_N, r = r, p = p };
                        stopwatch.Restart();
                        await _crypto.SCryptAsync(password, passwordSalt, parameters);
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