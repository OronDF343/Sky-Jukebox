using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;

namespace SkyJukebox.Widgets
{
    /// <summary>
    /// Interaction logic for QuickLoadWidget.xaml
    /// </summary>
    public partial class QuickLoadWidget
    {
        public QuickLoadWidget()
        {
            DisableAeroGlass = (bool)SettingsManager.Instance["DisableAeroGlass"].Value;
            InitializeComponent();
            AddFilesButton.Click += (sender, e) => 
            {
                DoFocusChange();
                InstanceManager.PlaylistEditorInstance.AddFiles_Click(sender, e);
            };
            AddFolderButton.Click += (sender, e) =>
            {
                DoFocusChange();
                InstanceManager.PlaylistEditorInstance.AddFolder_Click(sender, e);
            };
            OpenPlaylistButton.Click += (sender, e) =>
            {
                DoFocusChange();
                InstanceManager.PlaylistEditorInstance.OpenPlaylist_Click(sender, e);
            };
        }

        public QuickLoadWidget(Window parentWindow, Control showNear, WidgetRelativePosition relativePosition,
            WidgetAlignment alignment, bool allowOverlap, bool autoPosition)
            : this()
        {
            Initialize(parentWindow, showNear, relativePosition, alignment, allowOverlap, autoPosition);
        }

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                MainGrid.Focus();
        }

        public static IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }

        public static Brush BgBrush
        {
            get { return new SolidColorBrush(((System.Drawing.Color)SettingsManager.Instance["BgColor"].Value).ToWpfColor()); }
        }
    }
}
