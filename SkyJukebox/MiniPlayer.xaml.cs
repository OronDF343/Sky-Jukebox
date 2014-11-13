#region Using statements
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Utils;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.FlowDirection;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
#endregion

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public sealed partial class MiniPlayer : IDisposable
    {
        private NotifyIcon _controlNotifyIcon;
        public MiniPlayer()
        {
            DisableAeroGlass = Settings.Instance.DisableAeroGlass;
            InitializeComponent();
            InitNotifyIcon();
            SetAllIconImages();

            // Register important stuff:
            PlaybackManager.Instance.PlaybackEvent += UpdateScreen;
            PlaybackManager.Instance.TimerTickEvent += SetProgress;

            // Reposition window:
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? 0 : desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
            
            // Set colors:
            if (Settings.Instance.EnableRecolor)
            {
                SetIconColor(Color.FromArgb(Settings.Instance.GuiColor.R, Settings.Instance.GuiColor.G,
                    Settings.Instance.GuiColor.B));
                MainLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, Settings.Instance.GuiColor.R,
                    Settings.Instance.GuiColor.G, Settings.Instance.GuiColor.B));
            }

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
            _controlNotifyIcon.Icon = Properties.Icons.tg32i;
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
            PreviousButtonImage.Source = IconManager.Instance.GetIcon("previous32").ImageSource;
            PlayButtonImage.Source = IconManager.Instance.GetIcon(PlaybackManager.Instance.CurrentState == PlaybackManager.PlaybackStates.Playing ? "pause32" : "play32").ImageSource;
            NextButtonImage.Source = IconManager.Instance.GetIcon("next32").ImageSource;
            StopButtonImage.Source = IconManager.Instance.GetIcon("stop32").ImageSource;
            ShuffleButtonImage.Source = IconManager.Instance.GetIcon(PlaybackManager.Instance.Shuffle ? "shuffle32" : "shuffle32off").ImageSource;
            switch (PlaybackManager.Instance.LoopType)
            {
                case LoopTypes.Single:
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32single").ImageSource;
                    break;
                case LoopTypes.All:
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32all").ImageSource;
                    break;
                default:
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32none").ImageSource;
                    break;
            }
            OpenPlaylistButtonImage.Source = IconManager.Instance.GetIcon("playlist32").ImageSource;
            EditButtonImage.Source = IconManager.Instance.GetIcon("edit32").ImageSource;
            SettingsButtonImage.Source = IconManager.Instance.GetIcon("settings32").ImageSource;
            ColorButtonImage.Source = IconManager.Instance.GetIcon("color32").ImageSource;
            MinimizeButtonImage.Source = IconManager.Instance.GetIcon("minimize32").ImageSource;
            AboutButtonImage.Source = IconManager.Instance.GetIcon("info32").ImageSource;
            PowerButtonImage.Source = IconManager.Instance.GetIcon("exit32").ImageSource;
        }
        public void SetIconColor(Color c)
        {
            IconManager.Instance.SetRecolorAll(c);
            SetAllIconImages();
            MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
        }

        public void ResetIconColor()
        {
            IconManager.Instance.ResetColorAll();
            SetAllIconImages();
            MainLabel.Foreground = Brushes.Black;
        }

        public void SetProgressColor(Color c)
        {
            ProgressRectangle.Fill = new SolidColorBrush(c.ToWpfColor());
        }

        public void SetBgColor(Color c)
        {
            BgRectangle.Fill = new SolidColorBrush(c.ToWpfColor());
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Instance.ShowPlaylistEditorOnStartup)
            {
                var pe = new PlaylistEditorForm();
                pe.Show();
            }

            // Set the initial scrolling animation
            SetTextScrollingAnimation(MainLabel.Text);

            // Debug
            //MessageBox.Show("Actual size: " + playButtonImage.ActualHeight + "*" + playButtonImage.ActualWidth);

            // Open the file specified in CLArgs. If failed, open the autoload playlist if enabled
            if (!LoadFileFromClArgs() && Settings.Instance.LoadPlaylistOnStartup)
            {
                if (File.Exists(Settings.Instance.PlaylistToAutoLoad))
                    PlaybackManager.Instance.Playlist = new Playlist(Settings.Instance.PlaylistToAutoLoad);
                else
                    MessageBox.Show("File not found: " + Settings.Instance.PlaylistToAutoLoad,
                    "Non-critical error, everything is ok!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private bool LoadFileFromClArgs()
        {
            InstanceManager.CommmandLineArgs.RemoveAt(0);
            if (InstanceManager.CommmandLineArgs.Count == 0) return false;
            var file = InstanceManager.CommmandLineArgs.Find(s => !s.StartsWith("--"));
            if (file == default(string)) return false;

            if (Directory.Exists(file))
            {
                var dr = MessageBox.Show("Add files from the subfolders as well?", "Loading a directory",
                                         MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.Cancel) return false;
                PlaybackManager.Instance.Playlist.AddRange(file, dr == MessageBoxResult.Yes);
            }
            else if (!File.Exists(file))
            {
                MessageBox.Show("Invalid command line argument or file/directory not found: " + file,
                                "Non-critical error, everything is ok!", MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
                return false;
            }
            else
            {
                var ext = file.GetExt();
                if (ext.StartsWith("m3u")) // TODO: when other playlist format support is added, update this!
                {
                    if (InstanceManager.PlaylistEditorInstance == null)
                        InstanceManager.PlaylistEditorInstance = new PlaylistEditorForm();
                    if (InstanceManager.PlaylistEditorInstance.ClosePlaylistQuery())
                    {
                        PlaybackManager.Instance.Playlist = new Playlist(file);
                        InstanceManager.PlaylistEditorInstance.Dispose(); // TODO: Needs testing!
                    }
                    else
                        return false;
                }
                else if (PlaybackManager.Instance.HasSupportingPlayer(ext))
                    PlaybackManager.Instance.Playlist.Add(file);
                else
                {
                    MessageBox.Show("Unsupported file type: " + ext, "Non-critical error, everything is ok!",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }
            // by now we have determined that stuff was successfully added
            if (PlaybackManager.Instance.CurrentState != PlaybackManager.PlaybackStates.Playing)
                PlaybackManager.Instance.PlayPauseResume();
            return true;
        }

        #region Scrolling Text
        string _currentText;
        private void SetTextScrollingAnimation(string text)
        {
            if (_currentText == text) return;
            MainLabel.Text = _currentText = text;
            if (Settings.Instance.TextScrollingDelay <= 0) return;

            var copy = "       " + MainLabel.Text;
            var textGraphicalWidth = new FormattedText(copy, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(MainLabel.FontFamily.Source), MainLabel.FontSize, MainLabel.Foreground).WidthIncludingTrailingWhitespace;
            double textLengthGraphicalWidth = 0;
            //BorderTextBoxMarquee.Width = TextGraphicalWidth + 5;
            while (textLengthGraphicalWidth < MainLabel.ActualWidth)
            {
                MainLabel.Text = MainLabel.Text + copy;
                textLengthGraphicalWidth = new FormattedText(MainLabel.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(MainLabel.FontFamily.Source), MainLabel.FontSize, MainLabel.Foreground).WidthIncludingTrailingWhitespace;
            }
            MainLabel.Text += "       " + MainLabel.Text;
            var thickAnimation = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(-textGraphicalWidth, 0, 0, 0),
                RepeatBehavior = RepeatBehavior.Forever,
                Duration =
                    new Duration(
                        TimeSpan.FromSeconds(
                            StringUtils.Round(Settings.Instance.TextScrollingDelay * _currentText.Length)))
            };
            MainLabel.BeginAnimation(PaddingProperty, thickAnimation);
        }
        #endregion

        #region Single instance handling

        protected override IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Single instance: handling the message
            if (msg == ((App)Application.Current).Message)
                {
                    Show();
                    var args = ClArgs.GetClArgsFromFile();
                    InstanceManager.CommmandLineArgs = args.ToList();
                    //MessageBox.Show("Handled HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!LoadFileFromClArgs())
                        MessageBox.Show("Failed to load file: returned false", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            return base.HwndSourceHook(hwnd, msg, wParam, lParam, ref handled);
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PlaybackManager.Instance.Dispose();
            // Save window location:
            Settings.Instance.LastWindowLocation = new Point((int)Left, (int)Top);

            // Close all the things:
            if (InstanceManager.PlaylistEditorInstance != null)
                InstanceManager.PlaylistEditorInstance.Close();
            _controlNotifyIcon.Visible = false;
        }

        #region Clicks

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                MainGrid.Focus();
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
            ShuffleButtonImage.Source = IconManager.Instance.GetIcon(PlaybackManager.Instance.Shuffle ? "shuffle32" : "shuffle32off").ImageSource;
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            // TODO: move this to PlaybackManager
            switch (PlaybackManager.Instance.LoopType)
            {
                case LoopTypes.None:
                    PlaybackManager.Instance.LoopType = LoopTypes.Single;
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32single").ImageSource;
                    break;
                case LoopTypes.Single:
                    PlaybackManager.Instance.LoopType = LoopTypes.All;
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32all").ImageSource;
                    break;
                default:
                    PlaybackManager.Instance.LoopType = LoopTypes.None;
                    LoopButtonImage.Source = IconManager.Instance.GetIcon("loop32none").ImageSource;
                    break;
            }
        }
        private void openPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            var ofdiag = new OpenFileDialog { Filter = "Any M3U Playlist (*.m3u*)|*.m3u*", Multiselect = false };
            if (ofdiag.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            PlaybackManager.Instance.Playlist = new Playlist(ofdiag.FileName);
            SetTextScrollingAnimation("Playlist: " + ofdiag.FileName);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
			// Debug
            //new PlaylistEditor().Show();
            if (InstanceManager.PlaylistEditorInstance == null) InstanceManager.PlaylistEditorInstance = new PlaylistEditorForm();
            InstanceManager.PlaylistEditorInstance.Show();
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
            System.Windows.Forms.MessageBox.Show(StringUtils.GetSkyJukeboxAboutString(), "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void powerButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Close();
        }
        #endregion

        #region Updates
        private void UpdateScreen(object sender, PlaybackManager.PlaybackEventArgs e)
        {
            // Update play button image
            if (e.NewState == PlaybackManager.PlaybackStates.Playing)
            {
                PlayButtonImage.Source = IconManager.Instance.GetIcon("pause32").ImageSource;
                PlayButton.ToolTip = "Pause";
            }
            else
            {
                PlayButtonImage.Source = IconManager.Instance.GetIcon("play32").ImageSource;
                PlayButton.ToolTip = "Play";
            }

            // Update scrolling text
            SetTextScrollingAnimation(e.Message == "" ? StringUtils.FormatHeader(PlaybackManager.Instance.Playlist[e.NewTrackId], Settings.Instance.HeaderFormat) : e.Message);

            // Update NotifyIcon, show if MiniPlayer is hidden and playback is started
            _controlNotifyIcon.BalloonTipText = "Now Playing: " + e.NewTrackName;
            if (!IsVisible && e.NewState == PlaybackManager.PlaybackStates.Playing)
                _controlNotifyIcon.ShowBalloonTip(2000);
        }

        private void SetProgress(object sender, PlaybackManager.TimerTickEventArgs e)
        {
            FilledColumn.Width = new GridLength((long)e.Elapsed.TotalMilliseconds, GridUnitType.Star);
            var l = (long)e.Duration.TotalMilliseconds - (long)e.Elapsed.TotalMilliseconds;
            EmptyColumn.Width = new GridLength(l < 0 ? 0 : l, GridUnitType.Star);
        }
        #endregion

        public void Dispose()
        {
            _controlNotifyIcon.Dispose();
        }
    }
}
