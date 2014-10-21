using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SkyJukebox.Data;
using SkyJukebox.Icons;
using SkyJukebox.Playback;
using SkyJukebox.PluginAPI;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.FlowDirection;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Drawing.Size;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public sealed partial class MiniPlayer : IDisposable
    {
        private NotifyIcon _controlNotifyIcon;
        private string _lastPlaylist;
        public MiniPlayer()
        {
            InitializeComponent();
            InitNotifyIcon();

            // Error handling:
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) =>
                    MessageBox.Show(args.ExceptionObject.ToString(), "Fatal Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

            // Register important stuff:
            Instance.MiniPlayerInstance = this;
            PlaybackManager.Instance.PlaybackEvent += UpdateScreen;
            PlaybackManager.Instance.TimerTickEvent += SetProgress;
            // Load skins:
            if (!Directory.Exists(Instance.ExePath + Instance.SkinsPath))
                Directory.CreateDirectory(Instance.ExePath + Instance.SkinsPath);
            else
                SkinManager.Instance.LoadAllSkins(Instance.ExePath + Instance.SkinsPath);
            // Load settings:
            Settings.Init(Instance.ExePath + Instance.SettingsPath);
            // Set skin:
            if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
            {
                MessageBox.Show("Failed to load skin: " + Settings.Instance.SelectedSkin, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Settings.Instance.SelectedSkin.ResetValue();
                if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
                    MessageBox.Show("Failed to load fallback default skin!", "This is a bug!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            // Load plugins:
            PluginInteraction.RegisterAllPlugins();

            // Reposition window:
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? 0 : desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
            
            // Set colors:
            if (Settings.Instance.EnableRecolor)
                SetIconColor(Color.FromArgb(Settings.Instance.GuiColor.R, Settings.Instance.GuiColor.G, 
                    Settings.Instance.GuiColor.B));

            Background = Brushes.Transparent;
            SetProgressColor(Settings.Instance.ProgressColor);
            SetBgColor(Settings.Instance.BgColor);
        }

        #region NotifyIcon
        private void InitNotifyIcon()
        {
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
            _controlNotifyIcon.Icon = Properties.Icons.icon4261;
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
        }
        #endregion

        private void showButton_Click(object sender, EventArgs e)
        {
            Show();
        }

        #region Icon images and Color
        private void SetAllIconImages()
        {
            previousButtonImage.Source = IconManager.Instance.GetIcon("previous32").GetImageSource();
            playButtonImage.Source = IconManager.Instance.GetIcon("play32").GetImageSource();
            nextButtonImage.Source = IconManager.Instance.GetIcon("next32").GetImageSource();
            stopButtonImage.Source = IconManager.Instance.GetIcon("stop32").GetImageSource();
            shuffleButtonImage.Source = IconManager.Instance.GetIcon(PlaybackManager.Instance.Shuffle ? "shuffle32" : "shuffle32off").GetImageSource();
            switch (PlaybackManager.Instance.LoopType)
            {
                case PlaybackManager.LoopTypes.Single:
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32single").GetImageSource();
                    break;
                case PlaybackManager.LoopTypes.All:
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32all").GetImageSource();
                    break;
                default:
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32none").GetImageSource();
                    break;
            }
            openPlaylistButtonImage.Source = IconManager.Instance.GetIcon("playlist32").GetImageSource();
            editButtonImage.Source = IconManager.Instance.GetIcon("edit32").GetImageSource();
            settingsButtonImage.Source = IconManager.Instance.GetIcon("settings32").GetImageSource();
            colorButtonImage.Source = IconManager.Instance.GetIcon("color32").GetImageSource();
            minimizeButtonImage.Source = IconManager.Instance.GetIcon("minimize32").GetImageSource();
            aboutButtonImage.Source = IconManager.Instance.GetIcon("info32").GetImageSource();
            powerButtonImage.Source = IconManager.Instance.GetIcon("exit32").GetImageSource();
        }
        public void SetIconColor(Color c)
        {
            IconManager.Instance.SetRecolorAll(c);
            SetAllIconImages();
            mainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
        }

        public void ResetIconColor()
        {
            IconManager.Instance.ResetColorAll();
            SetAllIconImages();
            mainLabel.Foreground = Brushes.Black;
        }

        public void SetProgressColor(Color c)
        {
            ProgressRectangle.Fill = new SolidColorBrush(c.ToWpfColor());
        }

        public void ResetProgressColor()
        {
            ProgressRectangle.Fill = new SolidColorBrush(Settings.Instance.ProgressColor.DefaultValue.ToWpfColor());
        }

        public void SetBgColor(Color c)
        {
            BgRectangle.Fill = new SolidColorBrush(c.ToWpfColor());
        }
        public void ResetBgColor()
        {
            BgRectangle.Fill = new SolidColorBrush(Settings.Instance.BgColor.DefaultValue.ToWpfColor());
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Activate aero glass
            ActivateAeroGlass();

            if (Settings.Instance.ShowPlaylistEditorOnStartup)
            {
                var pe = new PlaylistEditor();
                pe.Show();
            }

            // Set the initial scrolling animation
            SetTextScrollingAnimation(mainLabel.Text);

            // Debug
            //MessageBox.Show("Actual size: " + playButtonImage.ActualHeight + "*" + playButtonImage.ActualWidth);

            // Open the file specified in CLArgs
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                //if (Settings.Instance.LoadPlaylistOnStartup && File.Exists(Settings.Instance.PlaylistToAutoLoad))
                //    PlaybackManager.Instance = new BackgroundPlayer(new Playlist(Settings.Instance.PlaylistToAutoLoad));
                //else
                //    PlaybackManager.Instance = new BackgroundPlayer();
                // TODO: Fix playlist autoloading
                return;
            }
            var file = args[1];
            if (!File.Exists(file))
            {
                MessageBox.Show("Invalid command line argument or file not found: " + file,
                    "Non-critical error, everything is ok!",
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            var ext = file.GetExt();
            if (ext.StartsWith("m3u")) // TODO: when other playlist format support is added, update this!
            {
                PlaybackManager.Instance.Playlist = new Playlist(file);
                _lastPlaylist = file;
            }
            else if (PlaybackManager.Instance.HasSupportingPlayer(ext))
                PlaybackManager.Instance.Playlist.Add(file);
            else
            {
                MessageBox.Show("Unsupported file type: " + ext, "Non-critical error, everything is ok!",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            PlaybackManager.Instance.PlayPauseResume();
        }

        #region Aero Glass
        private HwndSource _mainWindowSrc;
        private void ActivateAeroGlass()
        {
            if (Settings.Instance.DisableAeroGlass) return;
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
            if (Settings.Instance.TextScrollingDelay <= 0) return;

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
                            Util.Round(Settings.Instance.TextScrollingDelay * _currentText.Length)))
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
            // Save settings:
            Settings.Instance.LastWindowLocation = new Point((int)Left, (int)Top);
            Settings.SaveToXml();

            // Close all the things:
            if (Instance.PlaylistEditorInstance != null)
                Instance.PlaylistEditorInstance.Close();
            _controlNotifyIcon.Visible = false;
        }

        #region Clicks

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                mainGrid.Focus();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.PlayPauseResume();
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.Previous();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.Next();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.Stop();
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.Shuffle = !PlaybackManager.Instance.Shuffle;
            shuffleButtonImage.Source = IconManager.Instance.GetIcon(PlaybackManager.Instance.Shuffle ? "shuffle32" : "shuffle32off").GetImageSource();
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            // TODO: move this to PlaybackManager
            switch (PlaybackManager.Instance.LoopType)
            {
                case PlaybackManager.LoopTypes.None:
                    PlaybackManager.Instance.LoopType = PlaybackManager.LoopTypes.Single;
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32single").GetImageSource();
                    break;
                case PlaybackManager.LoopTypes.Single:
                    PlaybackManager.Instance.LoopType = PlaybackManager.LoopTypes.All;
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32all").GetImageSource();
                    break;
                default:
                    PlaybackManager.Instance.LoopType = PlaybackManager.LoopTypes.None;
                    loopButtonImage.Source = IconManager.Instance.GetIcon("loop32none").GetImageSource();
                    break;
            }
        }

        private void openPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            var ofdiag = new OpenFileDialog { Filter = "Any M3U Playlist (*.m3u*)|*.m3u*", Multiselect = false };
            if (ofdiag.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            PlaybackManager.Instance.Playlist = new Playlist(ofdiag.FileName);
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
            new Personalization().ShowDialog();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Hide();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            System.Windows.Forms.MessageBox.Show("Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.9.0 \"Modular\" Alpha2.0", "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void powerButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManager.Instance.Dispose();
            Close();
        }
        #endregion

        #region Updates
        private void UpdateScreen(object sender, PlaybackManager.PlaybackEventArgs e)
        {
            // Update play button image
            if (e.NewState == PlaybackManager.PlaybackStates.Playing)
            {
                playButtonImage.Source = IconManager.Instance.GetIcon("pause32").GetImageSource();
                playButton.ToolTip = "Pause";
            }
            else
            {
                playButtonImage.Source = IconManager.Instance.GetIcon("play32").GetImageSource();
                playButton.ToolTip = "Play";
            }

            // Update scrolling text
            SetTextScrollingAnimation(e.Message == "" ? Util.FormatHeader(PlaybackManager.Instance.Playlist[e.NewTrackId], Settings.Instance.HeaderFormat) : e.Message);

            // Update NotifyIcon, show if MiniPlayer is hidden and playback is started
            _controlNotifyIcon.BalloonTipText = "Now Playing: " + e.NewTrackName;
            if (!IsVisible && e.NewState == PlaybackManager.PlaybackStates.Playing)
                _controlNotifyIcon.ShowBalloonTip(2000);
        }

        private void SetProgress(object sender, PlaybackManager.TimerTickEventArgs e)
        {
            FilledColumn.Width = new GridLength((long)e.Elapsed.TotalMilliseconds, GridUnitType.Star);
            EmptyColumn.Width = new GridLength((long)e.Duration.TotalMilliseconds - (long)e.Elapsed.TotalMilliseconds, GridUnitType.Star);
        }
        #endregion

        public void Dispose()
        {
            _controlNotifyIcon.Dispose();
        }
    }
}
