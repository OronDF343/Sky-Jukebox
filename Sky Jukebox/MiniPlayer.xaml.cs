using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Size = System.Drawing.Size;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer
    {
        private readonly NotifyIcon _controlNotifyIcon;
        private Stopwatch _sw;
        private string _lastPlaylist;
        //readonly SplashScreen _spl = new SplashScreen();
        public MiniPlayer()
        {
            Instance.MiniPlayerInstance = this;

            //Hide();
            //var splashthread = new Thread(_spl.ShowSplashScreen) { IsBackground = true };
            //splashthread.Start();

            _sw = new Stopwatch();
            _sw.Start();

            InitializeComponent();

            // Reposition window:
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Left = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? 0 : desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;

            // Load data on startup:
            Util.LoadStuff();
            Instance.BgPlayer.PlaybackEvent += bgPlayer_PlaybackEvent;

            Background = Brushes.Transparent;

            #region NotifyIcon

            _controlNotifyIcon = new NotifyIcon();
            var iconContextMenuStrip = new ContextMenuStrip();
            var playPauseToolStripMenuItem = new ToolStripMenuItem();
            var previousToolStripMenuItem = new ToolStripMenuItem();
            var nextToolStripMenuItem = new ToolStripMenuItem();
            var stopToolStripMenuItem = new ToolStripMenuItem();
            var toolStripSeparator1 = new ToolStripSeparator();
            var showMiniPlayerToolStripMenuItem = new ToolStripMenuItem();
            var showPlaylistEditorToolStripMenuItem = new ToolStripMenuItem();
            var exitToolStripMenuItem = new ToolStripMenuItem();

            _controlNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _controlNotifyIcon.BalloonTipTitle = "Sky Jukebox";
            _controlNotifyIcon.ContextMenuStrip = iconContextMenuStrip;
            _controlNotifyIcon.Icon = Properties.Resources.icon4261;
            _controlNotifyIcon.Text = "Sky Jukebox";
            _controlNotifyIcon.Visible = true;
            _controlNotifyIcon.DoubleClick += (sender, e) => Show();

            iconContextMenuStrip.Items.AddRange(new ToolStripItem[] {
            playPauseToolStripMenuItem,
            previousToolStripMenuItem,
            nextToolStripMenuItem,
            stopToolStripMenuItem,
            toolStripSeparator1,
            showMiniPlayerToolStripMenuItem,
            showPlaylistEditorToolStripMenuItem,
            exitToolStripMenuItem});
            iconContextMenuStrip.Name = "iconContextMenuStrip";
            iconContextMenuStrip.Size = new Size(178, 164);
            // 
            // playPauseToolStripMenuItem
            // 
            playPauseToolStripMenuItem.Name = "playPauseToolStripMenuItem";
            playPauseToolStripMenuItem.Size = new Size(177, 22);
            playPauseToolStripMenuItem.Text = "Play/Pause";
            playPauseToolStripMenuItem.Click += playButton_Click;
            // 
            // previousToolStripMenuItem
            // 
            previousToolStripMenuItem.Name = "previousToolStripMenuItem";
            previousToolStripMenuItem.Size = new Size(177, 22);
            previousToolStripMenuItem.Text = "Previous";
            previousToolStripMenuItem.Click += previousButton_Click;
            // 
            // nextToolStripMenuItem
            // 
            nextToolStripMenuItem.Name = "nextToolStripMenuItem";
            nextToolStripMenuItem.Size = new Size(177, 22);
            nextToolStripMenuItem.Text = "Next";
            nextToolStripMenuItem.Click += nextButton_Click;
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Size = new Size(177, 22);
            stopToolStripMenuItem.Text = "Stop";
            stopToolStripMenuItem.Click += stopButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(174, 6);
            // 
            // showMiniPlayerToolStripMenuItem
            // 
            showMiniPlayerToolStripMenuItem.Font = new Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            showMiniPlayerToolStripMenuItem.Name = "showMiniPlayerToolStripMenuItem";
            showMiniPlayerToolStripMenuItem.Size = new Size(177, 22);
            showMiniPlayerToolStripMenuItem.Text = "Show MiniPlayer";
            showMiniPlayerToolStripMenuItem.Click += showButton_Click;
            // 
            // showPlaylistEditorToolStripMenuItem
            // 
            showPlaylistEditorToolStripMenuItem.Name = "showPlaylistEditorToolStripMenuItem";
            showPlaylistEditorToolStripMenuItem.Size = new Size(177, 22);
            showPlaylistEditorToolStripMenuItem.Text = "Show Playlist Editor";
            showPlaylistEditorToolStripMenuItem.Click += editButton_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(177, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += powerButton_Click;
            #endregion
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            Show();
        }

        private HwndSource _mainWindowSrc;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ActivateAeroGlass();

            _sw.Stop();
            //if (_sw.ElapsedMilliseconds < 1500)
            //    Thread.Sleep(1500 - _sw.ElapsedMilliseconds);
            //else
            //    Thread.Sleep(500);
            //Show();
            //_spl.CloseSplashScreen();
            //Activate();
            if (Instance.Settings.ShowPlaylistEditorOnStartup)
            {
                var pe = new PlaylistEditor();
                pe.Show();
            }

            SetTextScrollingAnimation(mainLabel.Text);
        }

        #region Aero Glass
        private void ActivateAeroGlass()
        {
            if (Instance.Settings.DisableAeroGlass) return;
            var windowInteropHelper = new WindowInteropHelper(this);
            var handle = windowInteropHelper.Handle;
            _mainWindowSrc = HwndSource.FromHwnd(handle);

            if (_mainWindowSrc == null || _mainWindowSrc.CompositionTarget == null)
            {
                MessageBox.Show("Error getting HwndSource! Window appearance can't be properly set!", "Exception in Loaded", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            _mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;
            var glassParams = new DwmApi.DwmBlurbehind
            {
                dwFlags = DwmApi.DwmBlurbehind.DWM_BB_ENABLE,
                fEnable = true,
                hRegionBlur = IntPtr.Zero
            };

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2)
            {
                var dis = 2;
                DwmApi.DwmSetWindowAttribute(_mainWindowSrc.Handle,
                    DwmApi.DwmWindowAttribute.DWMWA_LAST,
                    ref dis,
                    sizeof(uint));
            }
            DwmApi.DwmEnableBlurBehindWindow(
                handle,
                glassParams);
            //DwmApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, new DwmApi.Margins(0, 0, 0, 0));
        }
        #endregion

        #region Scrolling Text
        string currentText;
        private void SetTextScrollingAnimation(string text)
        {
            if (currentText == text) return;
            mainLabel.Text = currentText = text;
            if (Instance.Settings.TextScrollingDelay <= 0) return;

            string copy = "       " + mainLabel.Text;
            double textGraphicalWidth = new FormattedText(copy, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(mainLabel.FontFamily.Source), mainLabel.FontSize, mainLabel.Foreground).WidthIncludingTrailingWhitespace;
            double textLenghtGraphicalWidth = 0;
            //BorderTextBoxMarquee.Width = TextGraphicalWidth + 5;
            while (textLenghtGraphicalWidth < mainLabel.ActualWidth)
            {
                mainLabel.Text += copy;
                textLenghtGraphicalWidth = new FormattedText(mainLabel.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(mainLabel.FontFamily.Source), mainLabel.FontSize, mainLabel.Foreground).WidthIncludingTrailingWhitespace;
            }
            mainLabel.Text += "       " + mainLabel.Text;
            ThicknessAnimation thickAnimation = new ThicknessAnimation();
            thickAnimation.From = new Thickness(0, 0, 0, 0);
            thickAnimation.To = new Thickness(-textGraphicalWidth, 0, 0, 0);
            thickAnimation.RepeatBehavior = RepeatBehavior.Forever;
            thickAnimation.Duration = new Duration(TimeSpan.FromSeconds(Util.Round(Instance.Settings.TextScrollingDelay * (double)currentText.Length)));
            mainLabel.BeginAnimation(System.Windows.Controls.TextBox.PaddingProperty, thickAnimation);
        }
        #endregion

        #region Disable resizing the hacky way

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            if (source == null)
            {
                MessageBox.Show("Error getting HwndSource! Window appearance can't be properly set!", "Exception in SourceInitialized", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            source.AddHook(HwndSourceHook);

            var hWnd = new WindowInteropHelper(this).Handle;
            var flags = GetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/);
            SetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/, new IntPtr(flags.ToInt64() & ~(0x00010000L /*WS_MAXIMIZEBOX*/ | 0x00020000L /*WS_MINIMIZEBOX*/ | 0x00080000L /*WS_SYSMENU*/)));
        }

        private static IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0084 /*WM_NCHITTEST*/:
                    var result = DefWindowProc(hwnd, msg, wParam, lParam);
                    if (result.ToInt32() >= 10 /*HTLEFT*/ && result.ToInt32() <= 17 /*HTBOTTOMRIGHT*/ )
                    {
                        handled = true;
                        return new IntPtr(18 /*HTBORDER*/);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        // Drag the window:

        private const int WmNclbuttondown = 0xA1;
        private const int HtCaption = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            ReleaseCapture();
            SendMessage(_mainWindowSrc.Handle, WmNclbuttondown, HtCaption, 0);
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Instance.Settings.LastWindowLocation = new System.Drawing.Point((int)Left, (int)Top);
            Instance.Settings.SaveToXml();
            if (Instance.PlaylistEditorInstance != null)
                Instance.PlaylistEditorInstance.Close();
            Instance.BgPlayer.Dispose();
            Instance.BgPlayer = null;
            _controlNotifyIcon.Visible = false;
        }

        #region Clicks

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                mainLabel.Focus();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            switch (Instance.BgPlayer.Status)
            {
                case PlaybackStatus.Playing:
                case PlaybackStatus.Resumed:
                    Instance.BgPlayer.Pause();
                    break;
                case PlaybackStatus.Paused:
                    Instance.BgPlayer.Resume();
                    break;
                case PlaybackStatus.Stopped:
                    Instance.BgPlayer.Play();
                    break;
            }
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Instance.BgPlayer.Previous();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Instance.BgPlayer.Next();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Instance.BgPlayer.Stop();
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            Instance.BgPlayer.Shuffle = !Instance.BgPlayer.Shuffle;
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            switch (Instance.BgPlayer.LoopType)
            {
                case LoopType.None:
                    Instance.BgPlayer.LoopType = LoopType.Single;
                    break;
                case LoopType.Single:
                    Instance.BgPlayer.LoopType = LoopType.All;
                    break;
                default:
                    Instance.BgPlayer.LoopType = LoopType.None;
                    break;
            }
        }

        private void openPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            var ofdiag = new OpenFileDialog { Filter = "Any M3U Playlist (*.m3u*)|*.m3u*", Multiselect = false };
            if (ofdiag.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            Instance.BgPlayer.Playlist = new Playlist(ofdiag.FileName);
            _lastPlaylist = ofdiag.FileName;
            SetTextScrollingAnimation("Playlist: " + ofdiag.FileName);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            if (Instance.PlaylistEditorInstance == null) Instance.PlaylistEditorInstance = new PlaylistEditor();
            Instance.PlaylistEditorInstance.Show();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
        }

        private void zoomButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Hide();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            System.Windows.Forms.MessageBox.Show("Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.8.0", "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void powerButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Close();
        }
        #endregion

        void bgPlayer_PlaybackEvent(object sender, PlaybackEventArgs e)
        {
            UpdateScreen(e);
        }

        private void UpdateScreen(PlaybackEventArgs e)
        {
            if (e.NewStatus == PlaybackStatus.Playing || e.NewStatus == PlaybackStatus.Resumed)
                playButtonImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Icons/pause-icon-16.png"));
            else
                playButtonImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Icons/play-icon-16.png"));
            SetTextScrollingAnimation(Util.FormatHeader(Instance.BgPlayer.Playlist[e.NewTrackId], Instance.Settings.HeaderFormat));
            _controlNotifyIcon.BalloonTipText = "Now Playing: " + e.NewTrackName;
            if (!IsVisible)
                _controlNotifyIcon.ShowBalloonTip(2000);
        }
    }
}
