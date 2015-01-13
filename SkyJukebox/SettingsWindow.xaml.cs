using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Keyboard;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib.Icons;
using SkyJukebox.Utils;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private const string RegistryKey = "SkyJukeboxPlay";

        public SettingsWindow()
        {
            InitializeComponent();
            IsVisibleChanged += SettingsWindow_IsVisibleChanged;
            RecolorPicker.StandardColors.Remove(RecolorPicker.StandardColors.First(c => c.Color == Colors.Transparent));
            if (HasAdminRights)
            {
                try
                {
                    EnableFileContextMenu.IsChecked = FileShellExtension.GetIsRegistered("*", RegistryKey);
                    EnableFolderContextMenu.IsChecked = FileShellExtension.GetIsRegistered("Directory", RegistryKey);
                    FileContextMenuText.Text = FileShellExtension.GetRegisteredText("*", RegistryKey,
                                                                                    "Play with Sky Jukebox");
                    FolderContextMenuText.Text = FileShellExtension.GetRegisteredText("Directory", RegistryKey,
                                                                                    "Play with Sky Jukebox");
                }
                catch
                {
                }
            }
        }

        private BitmapSource _shieldIcon;
        public BitmapSource ShieldIcon { get { return _shieldIcon ?? (_shieldIcon = IconUtils.GetShieldIcon()); } }

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

        private bool? _hasAdminRights;
        public bool HasAdminRights
        {
            get
            {
                return _hasAdminRights ?? (bool)(_hasAdminRights = FileShellExtension.IsRunningAsAdministrator());
            }
        }
        public bool RestartRequired { get { return !HasAdminRights; } }

        #region Closing logic

        private bool _clicked;
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance.SaveEditAll();
            _clicked = true;
            Close();
            _clicked = false;
        }
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            SettingsInstance.DiscardEditAll();
            _clicked = true;
            Close();
            _clicked = false;
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

        private void RestartAsAdmin_OnClick(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo(InstanceManager.ExeFilePath)
            {
                Arguments = "--wait",
                Verb = "runas"
            };
            Process.Start(startInfo);
            InstanceManager.MiniPlayerInstance.Close();
        }

        private void UpdateContextMenus_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fireg = FileShellExtension.GetIsRegistered("*", RegistryKey);
                var fitext = FileShellExtension.GetRegisteredText("*", RegistryKey, null);
                if (fireg && (EnableFileContextMenu.IsChecked == false || fitext != FileContextMenuText.Text))
                    FileShellExtension.Unregister("*", RegistryKey);
                else if ((EnableFileContextMenu.IsChecked == true && !fireg) || fitext != FileContextMenuText.Text)
                    FileShellExtension.Register("*", RegistryKey, FileContextMenuText.Text, "\"" + InstanceManager.ExeFilePath + "\" \"%1\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update File Context Menu: " + ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

            try
            {
                var foreg = FileShellExtension.GetIsRegistered("Directory", RegistryKey);
                var fotext = FileShellExtension.GetRegisteredText("Directory", RegistryKey, null);
                if (foreg && (EnableFolderContextMenu.IsChecked == false || fotext != FolderContextMenuText.Text))
                    FileShellExtension.Unregister("Directory", RegistryKey);
                else if ((EnableFolderContextMenu.IsChecked == true && !foreg) || fotext != FolderContextMenuText.Text)
                    FileShellExtension.Register("Directory", RegistryKey, FileContextMenuText.Text, "\"" + InstanceManager.ExeFilePath + "\" \"%1\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update Folder Context Menu: " + ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void ForceUnregister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileShellExtension.Unregister("*", RegistryKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister File Context Menu: " + ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            EnableFileContextMenu.IsChecked = FileShellExtension.GetIsRegistered("*", RegistryKey);
            try
            {
                FileShellExtension.Unregister("Directory", RegistryKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister Folder Context Menu: " + ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            EnableFolderContextMenu.IsChecked = FileShellExtension.GetIsRegistered("Directory", RegistryKey);
        }
    }

    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(((decimal)value) * 100m);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) / 100m;
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
