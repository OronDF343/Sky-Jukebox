using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Keyboard;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public static Settings Settings
        {
            get { return Settings.Instance; }
        }

        public static Dictionary<Guid, string> OutputDevices
        {
            get { return AudioUtils.GetOutputDevicesInfo(); }
        }

        public static PlaybackManager PlaybackManager
        {
            get { return PlaybackManager.Instance; }
        }

        public static KeyBindingManager KeyBindingManager
        {
            get { return KeyBindingManager.Instance; }
        }

        public static SkinManager SkinManager
        {
            get { return SkinManager.Instance; }
        }

        public static string SelectedSkin
        {
            get { return Settings.SelectedSkin.Value; }
            set
            {
                Settings.SelectedSkin.Value = value;
                IconManager.Instance.LoadFromSkin(value);
            }
        }

        private void TextScrollingDelayDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.TextScrollingDelay.ResetValue();
        }

        private void NowPlayingFormatDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.HeaderFormat.ResetValue();
        }

        private void BgColorDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.BgColor.ResetValue();
        }

        private void ProgressColorDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.ProgressColor.ResetValue();
        }


        #region Closing logic

        private bool _clicked;
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.SaveToXml();
            _clicked = true;
            Close();
        }
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            Settings.DiscardChanges();
            _clicked = true;
            Close();
        }

        private bool _close;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_clicked && Visibility == Visibility.Visible)
            {
                var r = MessageBox.Show("Save changes?", "Sky Jukebox Settings", _close ? MessageBoxButton.YesNo : MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, _close ? MessageBoxResult.No : MessageBoxResult.Cancel);
                switch (r)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.No:
                        Settings.DiscardChanges();
                        break;
                    case MessageBoxResult.Yes:
                        Settings.SaveToXml();
                        break;
                }
            }
            if (_close) return;
            e.Cancel = true;
            Hide();
        }

        public void CloseFinal()
        {
            _close = true;
            Close();
        }
        #endregion
    }

    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) ((decimal) value*100m);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int) value/100m);
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((System.Drawing.Color) value).ToWpfColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Color)value).ToWinFormsColor();
        }
    }
}
