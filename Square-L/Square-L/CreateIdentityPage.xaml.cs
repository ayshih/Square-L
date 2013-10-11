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

namespace Square_L
{
    public partial class CreateIdentityPage : PhoneApplicationPage
    {
        private PhotoCamera _photoCamera;
        private byte[] _hash;
        private int _times;
        private Random _random;
        private byte[] _randomBytes;

        private SHA256Managed _SHA256;

        public CreateIdentityPage()
        {
            InitializeComponent();

            _times = 50;

            _random = new Random();
            _randomBytes = new byte[32];

            _SHA256 = new SHA256Managed();
            _hash = _SHA256.ComputeHash(BitConverter.GetBytes(DateTime.Now.Ticks));
            _random.NextBytes(_randomBytes);
            _hash = Xor(_hash, _SHA256.ComputeHash(_randomBytes));
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
                    _hash = Xor(_hash, _SHA256.ComputeHash(buffer));
                    _hash = Xor(_hash, _SHA256.ComputeHash(BitConverter.GetBytes(DateTime.Now.Ticks)));
                    _random.NextBytes(_randomBytes);
                    _hash = Xor(_hash, _SHA256.ComputeHash(_randomBytes));

                    ConnectingText.Text = Convert.ToBase64String(_hash);
                    Canvas.SetZIndex(Connecting, 1);

                    _times--;
                }
                catch { }
            }
            else
            {
                Directions.Text = "done rotating";
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
            HashBuffer();
        }

        public byte[] Xor(byte[] a, byte[] b)
        {
            int length = (a.Length < b.Length ? a.Length : b.Length);
            byte[] result = new byte[length];
            for (var i = 0; i < length; i++)
                result[i] = (byte)(a[i] ^ b[i]);
            return result;
        }
    }
}