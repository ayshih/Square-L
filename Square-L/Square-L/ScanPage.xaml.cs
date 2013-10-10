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

namespace Square_L
{
    public partial class ScanPage : PhoneApplicationPage
    {
        private readonly DispatcherTimer _timer;

        private PhotoLuminanceSource _luminance;
        private QRCodeReader _reader;
        private PhotoCamera _photoCamera;
        private AssembleUrl _assembleUrl;

        private byte[] _storedMasterKey;
        private byte[] _passwordSalt;
        private byte[] _passwordVerify;

        private CryptoRuntimeComponent _crypto;

        public ScanPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += (o, arg) => ScanBuffer();

            _crypto = new CryptoRuntimeComponent();

            _storedMasterKey = Convert.FromBase64String("VxXA0VcczUN6nj/9bMVlCeP7ogpqhmLCK54GIFTSl1s=");
            _passwordSalt = Convert.FromBase64String("Ze6tha++1E0=");
            _passwordVerify = Convert.FromBase64String("TlA6rTzAcCYWm8o/UF6sk3i8mU2JR/db34/6nE3HKDg=");
        }

        private void FocusCamera(object sender, EventArgs e)
        {
            _photoCamera.Focus();
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
            _photoCamera.Initialized += OnPhotoCameraInitialized;
            PreviewVideo.SetSource(_photoCamera);

            CameraButtons.ShutterKeyHalfPressed += FocusCamera;

            Debug.WriteLine("Stored master key: "+Base64UrlEncode(_storedMasterKey));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _timer.Stop();
            _photoCamera.Dispose();

            CameraButtons.ShutterKeyHalfPressed -= FocusCamera;

            base.OnNavigatingFrom(e);
        }

        private void OnPhotoCameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            int width = Convert.ToInt32(_photoCamera.PreviewResolution.Width);
            int height = Convert.ToInt32(_photoCamera.PreviewResolution.Height);

            _luminance = new PhotoLuminanceSource(width, height);
            _reader = new QRCodeReader();

            Dispatcher.BeginInvoke(() => _timer.Start());
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
                        _timer.Stop();
                        _photoCamera.Dispose();

                        ConnectingText.Text = _assembleUrl.DomainName;
                        Canvas.SetZIndex(Connecting, 1);

                        Directions.Text = "enter password";

                        PasswordGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch { }
        }

        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
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

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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

        private void ScanPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetLayout(e.Orientation);
            SetPreviewRotation(e.Orientation);
        }

        /// <summary>
        /// Encodes a byte array according to the base64url scheme
        /// </summary>
        /// <param name="input">byte array</param>
        /// <returns>encoded string</returns>
        public string Base64UrlEncode(byte[] input)
        {
            var result = Convert.ToBase64String(input);
            return result.Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// Decodes a string encoded with the base64url scheme into a byte array
        /// </summary>
        /// <param name="input">encoded string</param>
        /// <returns>byte array</returns>
        public byte[] Base64UrlDecode(string input)
        {
            var result = input.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(result);
        }

        public byte[] Xor(byte[] a, byte[] b)
        {
            int length = (a.Length < b.Length ? a.Length : b.Length);
            byte[] result = new byte[length];
            for (var i = 0; i < length; i++)
                result[i] = (byte)(a[i] ^ b[i]);
            return result;
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

                Dispatcher.BeginInvoke(() => VerifyPassword());
            }
        }

        private void VerifyPassword()
        {
            var password = System.Text.Encoding.UTF8.GetBytes(PasswordBox.Password);

            var scryptResult = new byte[32];
            _crypto.SCrypt(scryptResult, password, password.Length, _passwordSalt, 8, 14, 8, 1);
            Debug.WriteLine("SCrypt of password+salt: " + Base64UrlEncode(scryptResult));

            var passwordCheck = new byte[32];
            _crypto.SHA256(passwordCheck, scryptResult, 32);
            Debug.WriteLine("Password verify: " + Base64UrlEncode(_passwordVerify));
            Debug.WriteLine("Password check: " + Base64UrlEncode(passwordCheck));

            if (Base64UrlEncode(_passwordVerify).Equals(Base64UrlEncode(passwordCheck)))
            {
                var trueMasterKey = Xor(_storedMasterKey, scryptResult);
                Debug.WriteLine("True master key: " + Base64UrlEncode(trueMasterKey));

                var now = BitConverter.GetBytes(DateTime.Now.Ticks);
                var hash = new byte[32];
                _crypto.SHA256(hash, now, now.Length);
                _assembleUrl.AddParameter("sqrlnon", Base64UrlEncode(hash).Substring(0, 12));

                var seed = new byte[32];
                var DomainNameBytes = System.Text.Encoding.UTF8.GetBytes(_assembleUrl.DomainName);
                _crypto.HMAC_SHA256(seed, trueMasterKey, trueMasterKey.Length, DomainNameBytes, DomainNameBytes.Length);

                var publicKey = new byte[32];
                var privateKey = new byte[64];
                _crypto.CreateKeyPair(publicKey, privateKey, seed);
                Debug.WriteLine("Public key: " + Base64UrlEncode(publicKey));
                Debug.WriteLine("Private key: " + Base64UrlEncode(privateKey));

                var signature = new byte[64];
                var challenge = System.Text.Encoding.UTF8.GetBytes(_assembleUrl.Buffer);
                _crypto.CreateSignature(signature, challenge, challenge.Length, publicKey, privateKey);
                Debug.WriteLine("Challenge: " + _assembleUrl.Buffer);
                Debug.WriteLine("Signature: " + Base64UrlEncode(signature));

                _assembleUrl.AddParameter("sqrlsig", Base64UrlEncode(signature).TrimEnd('='));
                _assembleUrl.AddParameter("sqrlkey", Base64UrlEncode(publicKey).TrimEnd('='));

                _assembleUrl.AddParameter("sqrlver", "1.0");

                var query = (_assembleUrl.Protocol == "sqrl:" ? "https://" : "http://") + _assembleUrl.Buffer;

                var selection = MessageBox.Show(query, "Send SQRL login?", MessageBoxButton.OKCancel);
                if (selection == MessageBoxResult.OK)
                {
                    var webClient = new WebClient();
                    webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
                    webClient.DownloadStringAsync(new Uri(query));
                }
                else
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
            else
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
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