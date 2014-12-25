using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SkyJukebox.Core.Keyboard;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.SaveToXml();
        }
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            Settings.DiscardChanges();
        }
    }

    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((decimal)value * 100m);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value / 100m);
        }
    }
}
