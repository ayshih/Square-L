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
        private bool importIdentity;

        private Identity _identity;

        private CryptoRuntimeComponent _crypto;
        private Random _random;

        public ScanPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += (o, arg) => ScanBuffer();

            _crypto = new CryptoRuntimeComponent();
            _random = new Random();

            cameraActive = false;
            importIdentity = false;
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
                _identity = ((IdentityViewModel)DataContext).identity;
            }
            else
            {
                PageTitle.Text = "import identity";
                importIdentity = true;
            }

            SetLayout(((PhoneApplicationFrame)App.Current.RootVisual).Orientation);
            SetPreviewRotation(((PhoneApplicationFrame)App.Current.RootVisual).Orientation);

            _photoCamera = new PhotoCamera();
            SystemTray.ProgressIndicator.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Connecting to camera";
            _photoCamera.Initialized += OnPhotoCameraInitialized;
            PreviewVideo.SetSource(_photoCamera);

            CameraButtons.ShutterKeyHalfPressed += FocusCamera;
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
                    if (importIdentity)
                    {
                        if (!ParseIdentity(result.Text)) return;

                        ConnectingText.Text = "Identity found";
                    }
                    else
                    {
                        _assembleUrl = new AssembleUrl(result.Text);
                        if (!_assembleUrl.ValidSQRL()) return;

                        ConnectingText.Text = _assembleUrl.DomainName;
                    }

                    StopCamera();
                    Canvas.SetZIndex(Connecting, 1);

                    Directions.Text = "enter password";

                    PasswordGrid.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch { }
        }

        private bool ParseIdentity(string input)
        {
            var bytes = Base64Url.Decode(input);

            if ((bytes.Length != 78) || (bytes[0] != 1) || (bytes[33] != 1)) return false;

            _identity = new Identity(bytes);

           return true;
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
                Directions.Text = "please wait" + (importIdentity ? "\n(a long time)" : "");
                PasswordGrid.Visibility = System.Windows.Visibility.Collapsed;

                VerifyPassword();
            }
        }

        private async void VerifyPassword()
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            try
            {
                SystemTray.ProgressIndicator.Text = "Verifying password";
                var scryptResult = await _identity.GetSCryptResult(PasswordBox.Password);

                if (importIdentity)
                {
                    SystemTray.ProgressIndicator.Text = "Importing identity";
                    var newParameters = new Crypto.SCryptParameters { log2_N = 14, r = 8, p = 1 };
                    await _identity.ChangeSCryptParameters(PasswordBox.Password, newParameters, scryptResult);

                    _identity.nickname = Base64Url.Encode(_identity.passwordSalt);
                    var settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                    settings.Add("identity_" + _identity.nickname, _identity);
                    settings.Save();

                    App.ViewModel.Identities.Add(new IdentityViewModel() { identity = _identity });

                    NavigationService.GoBack();

                    return;
                }

                string query, post;
                _identity.GetQuery(_assembleUrl, scryptResult, out query, out post);

                SystemTray.ProgressIndicator.Text = "";

                var selection = MessageBox.Show(query + "\n\n" + post, "Send SQRL login?", MessageBoxButton.OKCancel);
                if (selection == MessageBoxResult.OK)
                {
                    Directions.Text = "sending authentication";

                    var webClient = new WebClient();

                    webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(ParseQueryResponse);
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                    webClient.UploadStringAsync(new Uri(query), post);
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The password you entered does not verify", "Password error", MessageBoxButton.OK);

                NavigationService.GoBack();
            }

            SystemTray.ProgressIndicator.IsVisible = false;
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