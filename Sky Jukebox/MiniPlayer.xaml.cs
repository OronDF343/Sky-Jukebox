using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using SkyJukebox.Display;
using SkyJukebox.Playback;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.FlowDirection;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using TextBox = System.Windows.Controls.TextBox;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public sealed partial class MiniPlayer : IDisposable
    {
        private readonly NotifyIcon _controlNotifyIcon;
        private readonly Stopwatch _sw;
        private string _lastPlaylist;
        private Color _currentColor;
        //readonly SplashScreen _spl = new SplashScreen();
        public MiniPlayer()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) =>
                    MessageBox.Show(args.ExceptionObject.ToString(), "Fatal Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
            Instance.MiniPlayerInstance = this;

            //Hide();
            //var splashthread = new Thread(_spl.ShowSplashScreen) { IsBackground = true };
            //splashthread.Start();

            _sw = new Stopwatch();
            _sw.Start();

            InitializeComponent();

            // Reposition window:
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? 0 : desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;

            // Load data on startup:
            Util.LoadStuff();
            //var args = Environment.GetCommandLineArgs();
            //for (int index = 1; index < args.Length; index += 2)
            //    Instance.CommmandLineArgs.Add(args[index], args[index + 1]);
            Instance.BgPlayer.PlaybackEvent += bgPlayer_PlaybackEvent;

            // Colors:
            CreateIconImages(
                _currentColor =
                    Instance.Settings.GuiColor == Color.FromArgb(0, 0, 0, 0)
                        ? Color.Black
                        : Color.FromArgb(Instance.Settings.GuiColor.R, Instance.Settings.GuiColor.G,
                            Instance.Settings.GuiColor.B));
            SetAllIconImages();

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

        #region Icon images
        private static void CreateIconImages(Color color)
        {
            // load icons:
            foreach (var g in Instance.IconUriDictionary)
            {
                MemoryStream ms = new MemoryStream();
                PngBitmapEncoder bbe = new PngBitmapEncoder();
                bbe.Frames.Add(BitmapFrame.Create(g.Value));
                bbe.Save(ms);
                Instance.IconImageDictionary.Remove(g.Key);
                Instance.IconImageDictionary.Add(g.Key, Image.FromStream(ms).RecolorFromGrayscale(color));
            }
        }

        private void SetAllIconImages()
        {
            previousButtonImage.SetIconImage("previous32");
            playButtonImage.SetIconImage("play32");
            nextButtonImage.SetIconImage("next32");
            stopButtonImage.SetIconImage("stop32");
            shuffleButtonImage.SetIconImage(Instance.BgPlayer.Shuffle ? "shuffle32" : "shuffle32off");
            switch (Instance.BgPlayer.LoopType)
            {
                case LoopType.Single:
                    loopButtonImage.SetIconImage("loop32single");
                    break;
                case LoopType.All:
                    loopButtonImage.SetIconImage("loop32all");
                    break;
                default:
                    loopButtonImage.SetIconImage("loop32none");
                    break;
            }
            openPlaylistButtonImage.SetIconImage("playlist32");
            editButtonImage.SetIconImage("edit32");
            settingsButtonImage.SetIconImage("settings32");
            colorButtonImage.SetIconImage("color32");
            minimizeButtonImage.SetIconImage("minimize32");
            aboutButtonImage.SetIconImage("info32");
            powerButtonImage.SetIconImage("exit32");
        }
        #endregion

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

            // Open the file specified in CLArgs
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2) return;

            var file = args[1];
            if (!File.Exists(file))
            {
                MessageBox.Show("Invalid command line argument or file not found: " + file,
                    "Non-critical error, everything is ok!",
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            var ext = file.SubstringRange(file.LastIndexOf('.'), file.Length - 1).ToLower();
            if (ext.StartsWith(".m3u")) // TODO: when other playlist format support is added, update this!
            {
                Instance.BgPlayer.Playlist = new Playlist(file);
                _lastPlaylist = file;
            }
            else
                Instance.BgPlayer.Playlist.Add(file);

            Instance.BgPlayer.Play();
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
            var glassParams = new NativeMethods.DwmBlurbehind
            {
                dwFlags = NativeMethods.DwmBlurbehind.DWM_BB_ENABLE,
                fEnable = true,
                hRegionBlur = IntPtr.Zero
            };

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2)
            {
                var dis = 2;
                NativeMethods.DwmSetWindowAttribute(_mainWindowSrc.Handle,
                    NativeMethods.DwmWindowAttribute.DWMWA_LAST,
                    ref dis,
                    sizeof(uint));
            }
            NativeMethods.DwmEnableBlurBehindWindow(
                handle,
                glassParams);
            //DwmApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, new DwmApi.Margins(0, 0, 0, 0));
        }
        #endregion

        #region Scrolling Text
        string _currentText;
        private void SetTextScrollingAnimation(string text)
        {
            if (_currentText == text) return;
            mainLabel.Text = _currentText = text;
            if (Instance.Settings.TextScrollingDelay <= 0) return;

            string copy = "       " + mainLabel.Text;
            double textGraphicalWidth = new FormattedText(copy, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(mainLabel.FontFamily.Source), mainLabel.FontSize, mainLabel.Foreground).WidthIncludingTrailingWhitespace;
            double textLengthGraphicalWidth = 0;
            //BorderTextBoxMarquee.Width = TextGraphicalWidth + 5;
            while (textLengthGraphicalWidth < mainLabel.ActualWidth)
            {
                mainLabel.Text = mainLabel.Text + copy;
                textLengthGraphicalWidth = new FormattedText(mainLabel.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(mainLabel.FontFamily.Source), mainLabel.FontSize, mainLabel.Foreground).WidthIncludingTrailingWhitespace;
            }
            mainLabel.Text += "       " + mainLabel.Text;
            var thickAnimation = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(-textGraphicalWidth, 0, 0, 0),
                RepeatBehavior = RepeatBehavior.Forever,
                Duration =
                    new Duration(
                        TimeSpan.FromSeconds(
                            Util.Round(Instance.Settings.TextScrollingDelay * _currentText.Length)))
            };
            mainLabel.BeginAnimation(PaddingProperty, thickAnimation);
        }
        #endregion

        #region Disable resizing the hacky way

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
            var flags = NativeMethods.GetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/);
#if WIN32
            var dwnl = flags & ~(0x00010000 /*WS_MAXIMIZEBOX*/| 0x00020000 /*WS_MINIMIZEBOX*/| 0x00080000 /*WS_SYSMENU*/);
#else
            var dwnl = new IntPtr(flags.ToInt64() & ~(0x00010000L /*WS_MAXIMIZEBOX*/| 0x00020000L /*WS_MINIMIZEBOX*/| 0x00080000L /*WS_SYSMENU*/));
#endif
            NativeMethods.SetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/, dwnl);
        }

        private static IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0084 /*WM_NCHITTEST*/:
                    var result = NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
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


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(_mainWindowSrc.Handle, NativeMethods.WmNclbuttondown, new IntPtr(NativeMethods.HtCaption), new IntPtr(0));
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Instance.Settings.LastWindowLocation = new Point((int)Left, (int)Top);
            Instance.Settings.SaveToXml();
            if (Instance.PlaylistEditorInstance != null)
                Instance.PlaylistEditorInstance.Close();
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
            shuffleButtonImage.SetIconImage(Instance.BgPlayer.Shuffle ? "shuffle32" : "shuffle32off");
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            switch (Instance.BgPlayer.LoopType)
            {
                case LoopType.None:
                    Instance.BgPlayer.LoopType = LoopType.Single;
                    loopButtonImage.SetIconImage("loop32single");
                    break;
                case LoopType.Single:
                    Instance.BgPlayer.LoopType = LoopType.All;
                    loopButtonImage.SetIconImage("loop32all");
                    break;
                default:
                    Instance.BgPlayer.LoopType = LoopType.None;
                    loopButtonImage.SetIconImage("loop32none");
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
            new SettingsForm().ShowDialog();
        }

        private void colorButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            if (_currentColor == Color.Black)
            {
                CreateIconImages(_currentColor = Color.White);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.White;
            }
            else if (_currentColor == Color.White)
            {
                CreateIconImages(_currentColor = Color.Red);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.Red;
            }
            else if (_currentColor == Color.Red)
            {
                CreateIconImages(_currentColor = Color.Green);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.Green;
            }
            else if (_currentColor == Color.Green)
            {
                CreateIconImages(_currentColor = Color.Blue);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.Blue;
            }
            else if (_currentColor == Color.Blue)
            {
                CreateIconImages(_currentColor = Color.Yellow);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.Yellow;
            }
            else
            {
                CreateIconImages(_currentColor = Color.Black);
                SetAllIconImages();
                mainLabel.Foreground = Brushes.Black;
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Hide();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            System.Windows.Forms.MessageBox.Show("Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.8.0 Alpha", "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void powerButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Instance.BgPlayer.Dispose();
            Instance.BgPlayer = null;
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
            {
                playButtonImage.SetIconImage("pause32");
                playButton.ToolTip = "Pause";
            }
            else
            {
                playButtonImage.SetIconImage("play32");
                playButton.ToolTip = "Play";
            }
            SetTextScrollingAnimation(e.Message == "" ? Util.FormatHeader(Instance.BgPlayer.Playlist[e.NewTrackId], Instance.Settings.HeaderFormat) : e.Message);
            _controlNotifyIcon.BalloonTipText = "Now Playing: " + e.NewTrackName;
            if (!IsVisible)
                _controlNotifyIcon.ShowBalloonTip(2000);
        }

        public void Dispose()
        {
            _controlNotifyIcon.Dispose();
        }
    }
}
