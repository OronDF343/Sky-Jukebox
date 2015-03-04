#region Using statements
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SkyJukebox.Api.Playback;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib.Icons;
using SkyJukebox.Lib.Wpf;
using SkyJukebox.Properties;
using SkyJukebox.Utils;
using SkyJukebox.Widgets;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.FlowDirection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;
#endregion

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public sealed partial class MiniPlayer : IDisposable, INotifyPropertyChanged
    {
        private NotifyIcon _controlNotifyIcon;
        public MiniPlayer()
        {
            AllowDrag = true;
            DisableAeroGlass = (bool)SettingsInstance["DisableAeroGlass"].Value;

            InitializeComponent();
            InitNotifyIcon();

            // Fix the color if Aero Glass is disabled:
            if (DisableAeroGlass && Background.Equals(System.Windows.Media.Brushes.Transparent))
                Background = new SolidColorBrush(System.Windows.SystemColors.ControlColor);

            // Register events:
            PlaybackManagerInstance.PropertyChanged += PlaybackManagerInstance_PropertyChanged;
            IconManagerInstance.CollectionChanged += (sender, args) => OnPropertyChanged("IconManagerInstance");
            SettingsInstance["GuiColor"].PropertyChanged += (sender, args) =>
            {
                if ((bool)SettingsInstance["EnableRecolor"].Value)
                {
                    var c = (Color)SettingsInstance["GuiColor"].Value;
                    IconManager.Instance.SetRecolorAll(c);
                    MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
                    ExtraTextLabel.Foreground = MainLabel.Foreground;
                }
            };
            SettingsInstance["EnableRecolor"].PropertyChanged += (sender, args) =>
            {
                if ((bool)SettingsInstance["EnableRecolor"].Value)
                {
                    var c = (Color)SettingsInstance["GuiColor"].Value;
                    IconManager.Instance.SetRecolorAll(c);
                    MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
                    ExtraTextLabel.Foreground = MainLabel.Foreground;
                }
                else
                {
                    IconManager.Instance.ResetColorAll();
                    // TODO: Set text color seperately
                    MainLabel.Foreground = new SolidColorBrush(Colors.Black);
                    ExtraTextLabel.Foreground = MainLabel.Foreground;
                }
            };
            PlaybackManagerInstance.Playlist.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove || args.Action == NotifyCollectionChangedAction.Reset)
                    OnPropertyChanged("ExtraText");
            };

            // Reposition window:
            if ((bool)SettingsInstance["RestoreLocation"].Value)
            {
                Left = ((Point)SettingsInstance["LastWindowLocation"].Value).X;
                Top = ((Point)SettingsInstance["LastWindowLocation"].Value).Y;
            }
            else
            {
                var desktopWorkingArea = SystemParameters.WorkArea;
                Left = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? 0 : desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
            }

            // Set colors:
            if ((bool)SettingsInstance["EnableRecolor"].Value)
            {
                var c = (Color)SettingsInstance["GuiColor"].Value;
                IconManager.Instance.SetRecolorAll(c);
                MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
                ExtraTextLabel.Foreground = MainLabel.Foreground;
            }

            // Update columns (so they work immediately):
            FilledColumnWidth = 0;
            EmptyColumnWidth = 1;
        }

        internal PluginsWidget PlWidget { get; private set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((bool)SettingsInstance["ShowPlaylistEditorOnStartup"].Value)
                InstanceManager.Instance.PlaylistEditorInstance.Show();

            // Set the initial scrolling animation
            SetTextScrollingAnimation(MainLabel.Text);

            // Init PluginWidget
            PlWidget = new PluginsWidget(this, PluginsButton, Widget.WidgetRelativePosition.Above,
                    Widget.WidgetAlignment.Center, false, true);

            // Load plugin GUIs:
            foreach (var p in InstanceManager.Instance.LoadedExtensions) p.Instance.InitGui();

            // Debug
            //MessageBox.Show("Actual size: " + playButtonImage.ActualHeight + "*" + playButtonImage.ActualWidth);

            // Open the file specified in CLArgs. If failed, open the autoload playlist if enabled
            if (!FileSystemUtils.LoadFileFromClArgs() && (bool)SettingsInstance["LoadPlaylistOnStartup"].Value)
            {
                var f = (string)SettingsInstance["PlaylistToAutoLoad"].Value;
                if (File.Exists(f))
                    PlaybackManagerInstance.Playlist.AddRange(f, FileSystemUtils.DefaultLoadErrorCallback);
                else
                    MessageBox.Show("File not found: " + f,
                    "Non-critical error, everything is ok!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

            try
            {
                // Update Checking
                if (!(bool)SettingsInstance["CheckForUpdates"].Value) return;
                var upd = await UpdateCheck.CheckForUpdate();
                if (upd == "") return;
                var result = MessageBox.Show("A new version of Sky Jukebox is available! Download the update now?",
                                             "Update Checker", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes) Process.Start("https://github.com/OronDF343/Sky-Jukebox/releases");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
            _controlNotifyIcon.Icon = Icons.tg32i;
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
            playPauseToolStripMenuItem.Text = "Play";
            playPauseToolStripMenuItem.Click += PlayButton_Click;
            // 
            // previousToolStripMenuItem
            // 
            previousToolStripMenuItem.Name = "previousToolStripMenuItem";
            previousToolStripMenuItem.Size = new Size(177, 22);
            previousToolStripMenuItem.Text = "Previous";
            previousToolStripMenuItem.Click += PreviousButton_Click;
            // 
            // nextToolStripMenuItem
            // 
            nextToolStripMenuItem.Name = "nextToolStripMenuItem";
            nextToolStripMenuItem.Size = new Size(177, 22);
            nextToolStripMenuItem.Text = "Next";
            nextToolStripMenuItem.Click += NextButton_Click;
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Size = new Size(177, 22);
            stopToolStripMenuItem.Text = "Stop";
            stopToolStripMenuItem.Click += StopButton_Click;
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
            showMiniPlayerToolStripMenuItem.Click += ShowButton_Click;
            // 
            // showPlaylistEditorToolStripMenuItem
            // 
            showPlaylistEditorToolStripMenuItem.Name = "showPlaylistEditorToolStripMenuItem";
            showPlaylistEditorToolStripMenuItem.Size = new Size(177, 22);
            showPlaylistEditorToolStripMenuItem.Text = "Show Playlist Editor";
            showPlaylistEditorToolStripMenuItem.Click += EditButton_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(177, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += PowerButton_Click;
        }
        #endregion

        private void ShowButton_Click(object sender, EventArgs e)
        {
            Show();
        }

        #region Binding stuff

        private void PlaybackManagerInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NowPlaying":
                    var h = StringUtils.FormatHeader(PlaybackManagerInstance.NowPlaying, (string)SettingsInstance["HeaderFormat"].Value);
                    // Update scrolling text
                    Dispatcher.Invoke(() => SetTextScrollingAnimation(h));
                    // Update NotifyIcon, show if MiniPlayer is hidden and playback is started
                    _controlNotifyIcon.BalloonTipText = h;
                    if (!IsVisible && PlaybackManagerInstance.CurrentState == PlaybackState.Playing)
                        _controlNotifyIcon.ShowBalloonTip(2000);
                    break;
                case "NowPlayingId":
                    OnPropertyChanged("ExtraText");
                    break;
                case "Duration":
                    var l2 = (int)PlaybackManagerInstance.Duration.TotalMilliseconds;
                    EmptyColumnWidth = l2 > 0 ? l2 : 1;
                    _tempDuration = PlaybackManagerInstance.Duration;
                    OnPropertyChanged("ExtraText");
                    break;
                case "Position":
                    l2 = (int)PlaybackManagerInstance.Position.TotalMilliseconds;
                    FilledColumnWidth = l2 < EmptyColumnWidth ? l2 : EmptyColumnWidth;
                    OnPropertyChanged("ExtraText");
                    break;
                case "CurrentState":
                    _controlNotifyIcon.ContextMenuStrip.Items["playPauseToolStripMenuItem"].Text =
                        PlaybackManagerInstance.CurrentState == PlaybackState.Playing
                            ? "Pause"
                            : PlaybackManagerInstance.CurrentState == PlaybackState.Paused
                                  ? "Resume"
                                  : "Play";
                    break;
            }
        }

        public SettingsManager SettingsInstance { get { return SettingsManager.Instance; } }
        public PlaybackManager PlaybackManagerInstance { get { return PlaybackManager.Instance; } }

        private double _filledLength;
        public double FilledColumnWidth
        {
            get { return _filledLength; }
            set
            {
                _filledLength = value;
                OnPropertyChanged("FilledColumnWidth");
            }
        }

        private double _emptyLength;
        public double EmptyColumnWidth
        {
            get { return _emptyLength; }
            set
            {
                var t = (int)_emptyLength;
                _emptyLength = value;
                if (t != (int)_emptyLength)
                    OnPropertyChanged("EmptyColumnWidth");
            }
        }

        public IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }
        #endregion

        #region Scrolling Text
        string _currentText;
        private void SetTextScrollingAnimation(string text)
        {
            if (_currentText == text) return;
            MainLabel.Text = _currentText = text;
            if ((double)SettingsInstance["TextScrollingDelay"].Value <= 0) return;

            var copy = "       " + MainLabel.Text;
            var textGraphicalWidth =
                new FormattedText(copy, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                  new Typeface(MainLabel.FontFamily, MainLabel.FontStyle, MainLabel.FontWeight,
                                               MainLabel.FontStretch), MainLabel.FontSize, MainLabel.Foreground)
                    .WidthIncludingTrailingWhitespace;
            double textLengthGraphicalWidth = 0;
            //BorderTextBoxMarquee.Width = TextGraphicalWidth + 5;
            while (textLengthGraphicalWidth < MainLabel.ActualWidth)
            {
                MainLabel.Text = MainLabel.Text + copy;
                textLengthGraphicalWidth =
                    new FormattedText(MainLabel.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                      new Typeface(MainLabel.FontFamily, MainLabel.FontStyle, MainLabel.FontWeight,
                                                   MainLabel.FontStretch), MainLabel.FontSize, MainLabel.Foreground)
                        .WidthIncludingTrailingWhitespace;
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
                            StringUtils.Round((double)SettingsInstance["TextScrollingDelay"].Value * _currentText.Length)))
            };
            MainLabel.BeginAnimation(PaddingProperty, thickAnimation);
        }
        #endregion

        #region ExtraText

        private TimeSpan _tempDuration;

        public string ExtraText 
        { 
            get
            {
                // Temporary fix for certain issues
                if (PlaybackManagerInstance.Duration != _tempDuration)
                {
                    var l2 = (int)PlaybackManagerInstance.Duration.TotalMilliseconds;
                    EmptyColumnWidth = l2 > 0 ? l2 : 1;
                }
                return string.Format("{0} / {1} > {2} / {3}",
                                     PlaybackManagerInstance.NowPlayingId + 1,
                                     PlaybackManagerInstance.Playlist.Count,
                                     PlaybackManagerInstance.Position.ToString(@"h\:mm\:ss"),
                                     PlaybackManagerInstance.Duration.ToString(@"h\:mm\:ss"));
            }
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
                    InstanceManager.Instance.CommmandLineArgs = args.ToList();
                    //MessageBox.Show("Handled HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!FileSystemUtils.LoadFileFromClArgs())
                        MessageBox.Show("Failed to load file: returned false", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            return base.HwndSourceHook(hwnd, msg, wParam, lParam, ref handled);
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (InstanceManager.Instance.PlaylistEditorInstance != null)
            {
                if (!InstanceManager.Instance.PlaylistEditorInstance.ClosePlaylistQuery())
                {
                    e.Cancel = true;
                    return;
                }
                InstanceManager.Instance.PlaylistEditorInstance.CloseFinal();
                InstanceManager.Instance.SettingsWindowInstance.CloseFinal();
            }

            if (_volWidget != null)
                _volWidget.Close();

            if (_qlWidget != null)
                _qlWidget.Close();

            if (PlWidget != null)
                PlWidget.Close();

            PlaybackManagerInstance.Dispose();

            SettingsInstance["LastWindowLocation"].Value = new Point((int)Left, (int)Top);

            _controlNotifyIcon.Visible = false;
        }

        #region Clicks

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                MainGrid.Focus();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManagerInstance.PlayPauseResume();
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManagerInstance.Previous();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManagerInstance.Next();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            PlaybackManagerInstance.Stop();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            PlaybackManagerInstance.Shuffle = !PlaybackManagerInstance.Shuffle;
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            switch (PlaybackManagerInstance.LoopType)
            {
                case LoopTypes.None:
                    PlaybackManagerInstance.LoopType = LoopTypes.Single;
                    break;
                case LoopTypes.Single:
                    PlaybackManagerInstance.LoopType = LoopTypes.All;
                    break;
                default:
                    PlaybackManagerInstance.LoopType = LoopTypes.None;
                    break;
            }
        }

        private VolumeWidget _volWidget;
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            if (_volWidget == null)
                _volWidget = new VolumeWidget(this, VolumeButton, Widget.WidgetRelativePosition.Above,
                    Widget.WidgetAlignment.Center, false, true);
            if (!_volWidget.IsVisible) _volWidget.Show();
            else _volWidget.Hide();
        }

        private void VolumeButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PlaybackManagerInstance.Volume == 0)
                PlaybackManagerInstance.Volume = InstanceManager.Instance.TempVolume > 0 ? InstanceManager.Instance.TempVolume : 1;
            else
            {
                InstanceManager.Instance.TempVolume = PlaybackManagerInstance.Volume;
                PlaybackManagerInstance.Volume = 0;
            }
        }

        private QuickLoadWidget _qlWidget;
        private void QuickLoadButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            if (_qlWidget == null)
                _qlWidget = new QuickLoadWidget(this, QuickLoadButton, Widget.WidgetRelativePosition.Above,
                    Widget.WidgetAlignment.Center, false, true);
            if (!_qlWidget.IsVisible) _qlWidget.Show();
            else _qlWidget.Hide();
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            InstanceManager.Instance.PlaylistEditorInstance.Show();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            InstanceManager.Instance.SettingsWindowInstance.Show();
        }

        private void PluginsButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            if (!PlWidget.IsVisible) PlWidget.Show();
            else PlWidget.Hide();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Hide();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            MessageBox.Show(StringUtils.GetSkyJukeboxAboutString(), "About Sky Jukebox", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PowerButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Close();
        }
        #endregion

        public void Dispose()
        {
            _controlNotifyIcon.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Seeking

        private readonly System.Windows.Controls.ToolTip _positionToolTip = new System.Windows.Controls.ToolTip{Placement = PlacementMode.Absolute};

        private void MiniPlayer_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift || !IsVisible) return;
            Panel.SetZIndex(BgProgressBar, 1);
            AllowDrag = false;
            _positionToolTip.Content = PositionToolTipText;
            BgProgressBar.ToolTip = _positionToolTip;
            var p = MouseUtils.CorrectGetPosition();
            _positionToolTip.HorizontalOffset = p.X;
            _positionToolTip.VerticalOffset = p.Y - 20;
            if (BgProgressBar.IsMouseOver)
                _positionToolTip.IsOpen = true;
        }

        private void MiniPlayer_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift) return;
            Panel.SetZIndex(BgProgressBar, -1);
            AllowDrag = true;
            _positionToolTip.IsOpen = false;
            BgProgressBar.ToolTip = null;
        }

        private void BgProgressBar_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Panel.GetZIndex(BgProgressBar) < 1 || !PlaybackManagerInstance.IsSomethingLoaded) return;
            var p = MouseUtils.CorrectGetPosition(BgProgressBar);
            try
            {
                PlaybackManagerInstance.Position =
                        TimeSpan.FromMilliseconds(PlaybackManagerInstance.Duration.TotalMilliseconds /
                                                  BgProgressBar.ActualWidth * p.X);
            }
            catch
            {
            }
        }

        private void BgProgressBar_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Panel.GetZIndex(BgProgressBar) < 1) return;
            _positionToolTip.Content = PositionToolTipText;
            _positionToolTip.IsOpen = BgProgressBar.IsMouseOver;
            if (!_positionToolTip.IsOpen) return;
            var p = MouseUtils.CorrectGetPosition();
            _positionToolTip.HorizontalOffset = p.X;
            _positionToolTip.VerticalOffset = p.Y - 20;
        }

        private string PositionToolTipText
        {
            get
            {
                if (Panel.GetZIndex(BgProgressBar) < 1) return null;
                var p = MouseUtils.CorrectGetPosition(BgProgressBar);
                try
                {
                    return TimeSpan.FromMilliseconds(PlaybackManagerInstance.Duration.TotalMilliseconds /
                                                              BgProgressBar.ActualWidth * p.X).ToString(@"h\:mm\:ss");
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion
    }

    public class BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(((Color)value).ToWpfColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color.ToWinFormsColor();
        }
    }
}
