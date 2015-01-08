#region Using statements
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SkyJukebox.Api;
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
            DisableAeroGlass = (bool)SettingsInstance["DisableAeroGlass"].Value;
            InitializeComponent();
            InitNotifyIcon();

            // Register events:
            PlaybackManager.Instance.PropertyChanged += PlaybackManagerInstance_PropertyChanged;
            IconManagerInstance.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged("IconManagerInstance");
                OnPropertyChanged("PlayButtonImage");
                OnPropertyChanged("ShuffleButtonImage");
                OnPropertyChanged("LoopButtonImage");
            };
            SettingsInstance["GuiColor"].PropertyChanged += (sender, args) =>
            {
                if ((bool)SettingsInstance["EnableRecolor"].Value)
                {
                    var c = (Color)SettingsInstance["GuiColor"].Value;
                    IconManager.Instance.SetRecolorAll(c);
                    MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
                }
            };
            SettingsInstance["EnableRecolor"].PropertyChanged += (sender, args) =>
            {
                if ((bool)SettingsInstance["EnableRecolor"].Value)
                {
                    var c = (Color)SettingsInstance["GuiColor"].Value;
                    IconManager.Instance.SetRecolorAll(c);
                    MainLabel.Foreground = new SolidColorBrush(c.ToWpfColor());
                }
                else
                {
                    IconManager.Instance.ResetColorAll();
                    // TODO: Set text color seperately
                    MainLabel.Foreground = new SolidColorBrush(Colors.Black);
                }
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
            }

            // Update columns (so they work immediately):
            FilledColumnWidth = 0;
            EmptyColumnWidth = 1;
        }

        private void PlaybackManagerInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentState":
                    OnPropertyChanged("PlayButtonImage");
                    OnPropertyChanged("PlayButtonToolTip");
                    break;
                case "Shuffle":
                    OnPropertyChanged("ShuffleButtonImage");
                    break;
                case "LoopType":
                    OnPropertyChanged("LoopButtonImage");
                    break;
                case "NowPlaying":
                    var h = StringUtils.FormatHeader(PlaybackManager.Instance.NowPlaying, (string)SettingsInstance["HeaderFormat"].Value);
                    // Update scrolling text
                    SetTextScrollingAnimation(h);
                    // Update NotifyIcon, show if MiniPlayer is hidden and playback is started
                    _controlNotifyIcon.BalloonTipText = h;
                    if (!IsVisible && PlaybackManager.Instance.CurrentState == PlaybackManager.PlaybackStates.Playing)
                        _controlNotifyIcon.ShowBalloonTip(2000);
                    break;
                case "Position":
                case "Duration":
                    var l2 = (int)PlaybackManager.Instance.Duration.TotalMilliseconds;
                    EmptyColumnWidth = l2 > 0 ? l2 : 1;
                    l2 = (int)PlaybackManager.Instance.Position.TotalMilliseconds;
                    FilledColumnWidth = l2 < EmptyColumnWidth ? l2 : EmptyColumnWidth;
                    break;
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

        #region Icon images and Colors

        public SettingsManager SettingsInstance { get { return SettingsManager.Instance; } }

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

        public ImageSource PlayButtonImage
        {
            get
            {
                return IconManagerInstance[PlaybackManager.Instance.CurrentState == PlaybackManager.PlaybackStates.Playing
                                               ? "pause32"
                                               : "play32"].ImageSource;
            }
        }

        public string PlayButtonToolTip
        {
            get
            {
                switch (PlaybackManager.Instance.CurrentState)
                {
                    case PlaybackManager.PlaybackStates.Playing:
                        return "Pause";
                    case PlaybackManager.PlaybackStates.Paused:
                        return "Resume";
                    default:
                        return "Play";
                }
            }
        }

        public ImageSource ShuffleButtonImage
        {
            get
            {
                return IconManagerInstance[PlaybackManager.Instance.Shuffle
                                               ? "shuffle32"
                                               : "shuffle32off"].ImageSource;
            }
        }

        public ImageSource LoopButtonImage
        {
            get
            {
                switch (PlaybackManager.Instance.LoopType)
                {
                    case LoopTypes.Single:
                        return IconManagerInstance["loop32single"].ImageSource;
                    case LoopTypes.All:
                        return IconManagerInstance["loop32all"].ImageSource;
                    default:
                        return IconManagerInstance["loop32none"].ImageSource;
                }
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((bool)SettingsInstance["ShowPlaylistEditorOnStartup"].Value)
                InstanceManager.PlaylistEditorInstance.Show();

            // Set the initial scrolling animation
            SetTextScrollingAnimation(MainLabel.Text);

            // Debug
            //MessageBox.Show("Actual size: " + playButtonImage.ActualHeight + "*" + playButtonImage.ActualWidth);

            // Open the file specified in CLArgs. If failed, open the autoload playlist if enabled
            if (!DirUtils.LoadFileFromClArgs() && (bool)SettingsInstance["LoadPlaylistOnStartup"].Value)
            {
                var f = (string)SettingsInstance["PlaylistToAutoLoad"].Value;
                if (File.Exists(f))
                    PlaybackManager.Instance.Playlist = new Playlist(f);
                else
                    MessageBox.Show("File not found: " + f,
                    "Non-critical error, everything is ok!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        

        #region Scrolling Text
        string _currentText;
        private void SetTextScrollingAnimation(string text)
        {
            if (_currentText == text) return;
            MainLabel.Text = _currentText = text;
            if ((double)SettingsInstance["TextScrollingDelay"].Value <= 0) return;

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
                            StringUtils.Round((double)SettingsInstance["TextScrollingDelay"].Value * _currentText.Length)))
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
                    if (!DirUtils.LoadFileFromClArgs())
                        MessageBox.Show("Failed to load file: returned false", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            return base.HwndSourceHook(hwnd, msg, wParam, lParam, ref handled);
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (InstanceManager.PlaylistEditorInstance != null)
            {
                if (!InstanceManager.PlaylistEditorInstance.ClosePlaylistQuery())
                {
                    e.Cancel = true;
                    return;
                }
                InstanceManager.PlaylistEditorInstance.CloseFinal();
                InstanceManager.SettingsWindowInstance.CloseFinal();
            }

            if (_qlWidget != null)
                _qlWidget.Close();

            if (_plWidget != null)
                _plWidget.Close();

            PlaybackManager.Instance.Dispose();

            SettingsInstance["LastWindowLocation"].Value = new Point((int)Left, (int)Top);

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
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            switch (PlaybackManager.Instance.LoopType)
            {
                case LoopTypes.None:
                    PlaybackManager.Instance.LoopType = LoopTypes.Single;
                    break;
                case LoopTypes.Single:
                    PlaybackManager.Instance.LoopType = LoopTypes.All;
                    break;
                default:
                    PlaybackManager.Instance.LoopType = LoopTypes.None;
                    break;
            }
        }

        private QuickLoadWidget _qlWidget;
        private void quickLoadButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            if (_qlWidget == null)
                _qlWidget = new QuickLoadWidget(this, QuickLoadButton, Widget.WidgetRelativePosition.Above,
                    Widget.WidgetAlignment.Center, false, true);
            _qlWidget.Show();
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            InstanceManager.PlaylistEditorInstance.Show();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            //new SettingsForm().ShowDialog();
            InstanceManager.SettingsWindowInstance.Show();
        }

        private PluginsWidget _plWidget;
        private void pluginsButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            //new Personalization().ShowDialog();
            if (_plWidget == null)
            {
                _plWidget = new PluginsWidget(this, PluginsButton, Widget.WidgetRelativePosition.Above,
                    Widget.WidgetAlignment.Center, false, true);
                foreach (var ei in InstanceManager.LoadedPlugins)
                    _plWidget.AddButton(ei);
            }
            _plWidget.Show();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            DoFocusChange();
            Hide();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            DoFocusChange();
            MessageBox.Show(StringUtils.GetSkyJukeboxAboutString(), "About Sky Jukebox", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void powerButton_Click(object sender, EventArgs e)
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

        private void MiniPlayer_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift && IsVisible)
                Panel.SetZIndex(BgProgressBar, 1);
        }

        private void MiniPlayer_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                Panel.SetZIndex(BgProgressBar, -1);
        }

        private void BgProgressBar_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var p = MouseUtils.CorrectGetPosition(BgProgressBar);
            try
            {
                // TODO: add indication if a file is currently loaded correctly
                PlaybackManager.Instance.Position =
                        TimeSpan.FromMilliseconds(PlaybackManager.Instance.Duration.TotalMilliseconds /
                                                  BgProgressBar.ActualWidth * p.X);
            }
            catch
            {
            }
        }
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
