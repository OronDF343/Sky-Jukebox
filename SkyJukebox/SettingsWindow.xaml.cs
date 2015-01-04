using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Keyboard;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib.Icons;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;

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
            IsVisibleChanged += SettingsWindow_IsVisibleChanged;
            RecolorPicker.StandardColors.Remove(RecolorPicker.StandardColors.First(c => c.Color == Colors.Transparent));
        }

        private void SettingsWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !e.NewValue.Equals(e.OldValue))
                SettingsInstance.BeginEditAll();
        }

        public static SettingsManager SettingsInstance
        {
            get { return SettingsManager.Instance; }
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

        public string SelectedSkin
        {
            get { return (string)SettingsInstance["SelectedSkin"].Value; }
            set
            {
                SettingsInstance["SelectedSkin"].Value = value;
                IconManager.Instance.LoadFromSkin(value);
            }
        }

        private void TextScrollingDelayDefault_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance["TextScrollingDelay"].ResetValue();
        }

        private void NowPlayingFormatDefault_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance["HeaderFormat"].ResetValue();
        }

        private void BgColorDefault_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance["BgColor"].ResetValue();
        }

        private void ProgressColorDefault_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance["ProgressColor"].ResetValue();
        }


        #region Closing logic

        private bool _clicked;
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance.SaveEditAll();
            _clicked = true;
            Close();
        }
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance.DiscardEditAll();
            _clicked = true;
            Close();
        }

        private bool _close;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_clicked && IsVisible)
            {
                var r = MessageBox.Show("Save changes?", "Sky Jukebox Settings",
                    _close ? MessageBoxButton.YesNo : MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, _close ? MessageBoxResult.No : MessageBoxResult.Cancel);
                switch (r)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.No:
                        SettingsInstance.DiscardEditAll();
                        break;
                    case MessageBoxResult.Yes:
                        SettingsInstance.SaveEditAll();
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
            return ((Color) value).ToWpfColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((System.Windows.Media.Color)value).ToWinFormsColor();
        }
    }
}
