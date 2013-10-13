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
using Microsoft.Devices;
using System.Windows.Threading;
using Crypto;

namespace Square_L
{
    public partial class CreateIdentityPage : PhoneApplicationPage
    {
        private PhotoCamera _photoCamera;
        private byte[] _hashRandom, _hashImages;
        private int _times;
        private Random _random;
        private byte[] _randomBytes;

        private bool _cameraActive;
        private bool _readyToSave;

        private SHA256Managed _SHA256;
        private CryptoRuntimeComponent _crypto;

        public CreateIdentityPage()
        {
            InitializeComponent();

            _times = 50;

            _cameraActive = false;
            _readyToSave = false;

            _random = new Random();
            _randomBytes = new byte[32];

            _SHA256 = new SHA256Managed();
            _hashImages = _SHA256.ComputeHash(BitConverter.GetBytes(DateTime.Now.Ticks));
            _random.NextBytes(_randomBytes);
            _hashRandom = _SHA256.ComputeHash(_randomBytes);

            _crypto = new CryptoRuntimeComponent();
        }

        private void StopCamera()
        {
            _photoCamera.Dispose();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _photoCamera = new PhotoCamera();
            _photoCamera.Initialized += OnPhotoCameraInitialized;
            PreviewVideo.SetSource(_photoCamera);
       }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            StopCamera();

            base.OnNavigatingFrom(e);
        }

        private void OnPhotoCameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            _cameraActive = true;
        }

        private void HashBuffer()
        {
            if (_times > 0)
            {
                try
                {
                    Directions.Text = "rotate " + _times.ToString() + " more time" + (_times != 1 ? "s" : "");

                    int width = Convert.ToInt32(_photoCamera.PreviewResolution.Width);
                    int height = Convert.ToInt32(_photoCamera.PreviewResolution.Height);

                    var buffer = new byte[width*height];
                    _photoCamera.GetPreviewBufferY(buffer);
                    var time = BitConverter.GetBytes(DateTime.Now.Ticks);
                    Buffer.BlockCopy(time, 0, buffer, 0, time.Length);
                    _hashImages = Utility.Xor(_hashImages, _SHA256.ComputeHash(buffer));

                    _random.NextBytes(_randomBytes);
                    _hashRandom = Utility.Xor(_hashRandom, _SHA256.ComputeHash(_randomBytes));

                    //ConnectingText.Text = Convert.ToBase64String(_hash);
                    //Canvas.SetZIndex(Connecting, 1);

                    _times--;
                }
                catch { }
            }
            else
            {
                if (!_readyToSave)
                {
                    _readyToSave = true;
                    Directions.Visibility = System.Windows.Visibility.Collapsed;
                    IdentityGrid.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Rearranges the elements on the page depending on the page orientation
        /// </summary>
        /// <param name="orientation">page orientation</param>
        private void SetLayout(PageOrientation orientation)
        {
            switch (orientation & PageOrientation.Landscape)
            {
                case PageOrientation.Landscape:
                    Grid.SetColumnSpan(Preview, 1);
                    Preview.Width = 400;
                    Preview.Height = 300;
                    Grid.SetColumnSpan(Connecting, 1);
                    Grid.SetRow(Other, 0);
                    Grid.SetColumn(Other, 1);
                    Grid.SetColumnSpan(Other, 1);
                    break;
                default:
                    Grid.SetColumnSpan(Preview, 2);
                    Preview.Width = 300;
                    Preview.Height = 400;
                    Grid.SetColumnSpan(Connecting, 2);
                    Grid.SetRow(Other, 1);
                    Grid.SetColumn(Other, 0);
                    Grid.SetColumnSpan(Other, 2);
                    break;
            }
        }

        private void SetPreviewRotation(PageOrientation orientation)
        {
            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                    PreviewTransform.Rotation = 0;
                    break;
                case PageOrientation.LandscapeRight:
                    PreviewTransform.Rotation = 180;
                    break;
                default:
                    PreviewTransform.Rotation = 90;
                    break;
            }
        }

        private void CreateIdentityPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetLayout(e.Orientation);
            SetPreviewRotation(e.Orientation);
            if (_cameraActive) HashBuffer();
        }

        private void IdentitySave_Click(object sender, RoutedEventArgs e)
        {
            if (NicknameBox.Text != "")
            {
                SystemTray.ProgressIndicator.IsVisible = true;

                IdentityGrid.Visibility = System.Windows.Visibility.Collapsed;

                Dispatcher.BeginInvoke(() => SaveIdentity());
            }
        }

        private void SaveIdentity()
        {
            var password = System.Text.Encoding.UTF8.GetBytes(PasswordBox.Password);

            var passwordSalt = new byte[8];
            _random.NextBytes(passwordSalt);

            var scryptResult = new byte[32];
            _crypto.SCrypt(scryptResult, password, passwordSalt, 14, 8, 1);

            var passwordHash = _SHA256.ComputeHash(scryptResult);

            var masterKey = new byte[32];
            _crypto.PBKDF2_HMACSHA256(masterKey, _hashImages, _hashRandom, 1000000);
            masterKey = Utility.Xor(masterKey, scryptResult);

            var identity = new Identity() { nickname = NicknameBox.Text, lastUsed = new DateTime(0), masterKey = masterKey, passwordSalt = passwordSalt, passwordHash = passwordHash };

            var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
            settings.Add("identity_" + identity.nickname, identity);
            settings.Save();

            App.ViewModel.Identities.Add(new IdentityViewModel() { identity = identity });

            NavigationService.GoBack();
        }
    }
}