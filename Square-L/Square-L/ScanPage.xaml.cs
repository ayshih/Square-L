using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using ZXing.QrCode;
using System.Windows.Threading;
using ZXing;
using Crypto;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Square_L
{
    public partial class ScanPage : PhoneApplicationPage
    {
        private readonly DispatcherTimer _timer;

        private PhotoLuminanceSource _luminance;
        private QRCodeReader _reader;
        private PhotoCamera _photoCamera;
        private AssembleUrl _assembleUrl;

        private bool cameraActive;

        private CryptoRuntimeComponent _crypto;

        public ScanPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += (o, arg) => ScanBuffer();

            _crypto = new CryptoRuntimeComponent();
        }

        private void FocusCamera(object sender, EventArgs e)
        {
            if (cameraActive) _photoCamera.Focus();
        }

        private void StopCamera()
        {
            _timer.Stop();
            _photoCamera.Dispose();
            cameraActive = false;
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

            SetLayout(((PhoneApplicationFrame)App.Current.RootVisual).Orientation);
            SetPreviewRotation(((PhoneApplicationFrame)App.Current.RootVisual).Orientation);

            _photoCamera = new PhotoCamera();
            SystemTray.ProgressIndicator.IsVisible = true;
            _photoCamera.Initialized += OnPhotoCameraInitialized;
            PreviewVideo.SetSource(_photoCamera);

            CameraButtons.ShutterKeyHalfPressed += FocusCamera;

            Debug.WriteLine("Stored master key: " + Base64Url.Encode(((IdentityViewModel)DataContext).masterKey));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            StopCamera();

            CameraButtons.ShutterKeyHalfPressed -= FocusCamera;

            base.OnNavigatingFrom(e);
        }

        private void OnPhotoCameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            cameraActive = true;

            int width = Convert.ToInt32(_photoCamera.PreviewResolution.Width);
            int height = Convert.ToInt32(_photoCamera.PreviewResolution.Height);

            _luminance = new PhotoLuminanceSource(width, height);
            _reader = new QRCodeReader();

            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = false;
                _timer.Start();
            });
        }

        private void ScanBuffer()
        {
            try
            {
                _photoCamera.GetPreviewBufferY(_luminance.FrameY);
                var binarizer = new ZXing.Common.HybridBinarizer(_luminance);
                var binBitmap = new BinaryBitmap(binarizer);
                var result = _reader.decode(binBitmap);

                if (result != null)
                {
                    _assembleUrl = new AssembleUrl(result.Text);
                    if (_assembleUrl.ValidSQRL())
                    {
                        StopCamera();

                        ConnectingText.Text = _assembleUrl.DomainName;
                        Canvas.SetZIndex(Connecting, 1);

                        Directions.Text = "enter password";

                        PasswordGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch { }
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

        private void ScanPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetLayout(e.Orientation);
            SetPreviewRotation(e.Orientation);
        }

        private void PasswordLogin_Click(object sender, RoutedEventArgs e)
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
                Directions.Text = "verifying password";
                PasswordGrid.Visibility = System.Windows.Visibility.Collapsed;

                SystemTray.ProgressIndicator.IsVisible = true;

                Dispatcher.BeginInvoke(() => VerifyPassword());
            }
        }

        private void VerifyPassword()
        {
            var password = System.Text.Encoding.UTF8.GetBytes(PasswordBox.Password);

            Debug.WriteLine("Password salt: " + Base64Url.Encode(((IdentityViewModel)DataContext).passwordSalt));

            var scryptResult = new byte[32];
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _crypto.SCrypt(scryptResult, password, ((IdentityViewModel)DataContext).passwordSalt, 14, 8, 1);
            stopwatch.Stop();
            Debug.WriteLine("SCrypt of password+salt: " + Base64Url.Encode(scryptResult) + " (" + stopwatch.ElapsedMilliseconds.ToString() + " ms)");

            var _SHA256 = new SHA256Managed();

            var passwordCheck = _SHA256.ComputeHash(scryptResult);
            Debug.WriteLine("Password hash: " + Base64Url.Encode(((IdentityViewModel)DataContext).passwordHash));
            Debug.WriteLine("Password check: " + Base64Url.Encode(passwordCheck));

            if (Base64Url.Encode(passwordCheck).Equals(Base64Url.Encode(((IdentityViewModel)DataContext).passwordHash)))
            {
                var trueMasterKey = Utility.Xor(((IdentityViewModel)DataContext).masterKey, scryptResult);
                Debug.WriteLine("True master key: " + Base64Url.Encode(trueMasterKey));

                var DomainNameInBytes = System.Text.Encoding.UTF8.GetBytes(_assembleUrl.DomainName);
                var _HMACSHA256 = new HMACSHA256(trueMasterKey);
                var seed = _HMACSHA256.ComputeHash(DomainNameInBytes);
                Debug.WriteLine("Seed: " + Base64Url.Encode(seed));

                var publicKey = new byte[32];
                var privateKey = new byte[64];
                _crypto.CreateKeyPair(publicKey, privateKey, seed);
                Debug.WriteLine("Public key: " + Base64Url.Encode(publicKey));
                Debug.WriteLine("Private key: " + Base64Url.Encode(privateKey));

                _assembleUrl.AddParameter("sqrlver", "1");
                _assembleUrl.AddParameter("sqrlurl", _assembleUrl.Protocol);
                _assembleUrl.AddParameter("sqrlopt", "");
                _assembleUrl.AddParameter("sqrlkey", Base64Url.Encode(publicKey));

                var challenge = System.Text.Encoding.UTF8.GetBytes(_assembleUrl.Buffer);
                var query = (_assembleUrl.Protocol == "sqrl" ? "https://" : "http://") + _assembleUrl.Buffer;
                Debug.WriteLine("Challenge: " + _assembleUrl.Buffer);

                var signature = new byte[64];
                _crypto.CreateSignature(signature, challenge, publicKey, privateKey);
                Debug.WriteLine("Signature: " + Base64Url.Encode(signature));

                var parameters = "sqrlsig=" + Base64Url.Encode(signature);
                Debug.WriteLine("Parameters: " + parameters);

                var selection = MessageBox.Show(query+"\n\n"+parameters, "Send SQRL login?", MessageBoxButton.OKCancel);
                if (selection == MessageBoxResult.OK)
                {
                    Directions.Text = "sending authentication";

                    var webClient = new WebClient();

                    webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(ParseQueryResponse);
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                    webClient.UploadStringAsync(new Uri(query), parameters);
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
            else
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);

                NavigationService.GoBack();
            }
        }

        private void ParseQueryResponse(object sender, UploadStringCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    MessageBox.Show(e.Result, "Server response", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show(e.Error.Message, "Communication error", MessageBoxButton.OK);
                }

                NavigationService.GoBack();
            }
        }

        private void Preview_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FocusCamera(sender, e);
        }
    }

    public class PhotoLuminanceSource : LuminanceSource
    {
        public byte[] FrameY { get; private set; }

        public PhotoLuminanceSource(int width, int height)
            : base(width, height)
        {
            FrameY = new byte[width * height];
        }

        public override byte[] Matrix
        {
            get { return (byte[])(Array)FrameY; }
        }

        public override byte[] getRow(int y, byte[] row)
        {
            if (row == null || row.Length < Width)
            {
                row = new byte[Width];
            }

            for (var i = 0; i < Height; i++)
                row[i] = (byte)FrameY[i * Width + y];

            return row;
        }
    }
}