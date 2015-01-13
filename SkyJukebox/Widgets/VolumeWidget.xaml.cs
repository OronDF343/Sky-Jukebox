using System.Windows;
using System.Windows.Controls;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Xml;

namespace SkyJukebox.Widgets
{
    /// <summary>
    /// Interaction logic for VolumeWidget.xaml
    /// </summary>
    public partial class VolumeWidget
    {
        public VolumeWidget()
        {
            DisableAeroGlass = (bool)SettingsManager.Instance["DisableAeroGlass"].Value;
            InitializeComponent();
        }

        public VolumeWidget(Window parentWindow, Control showNear, WidgetRelativePosition relativePosition,
            WidgetAlignment alignment, bool allowOverlap, bool autoPosition)
            : this()
        {
            Initialize(parentWindow, showNear, relativePosition, alignment, allowOverlap, autoPosition);
        }

        public SettingsManager SettingsInstance { get { return SettingsManager.Instance; } }

        public static PlaybackManager PlaybackManager
        {
            get { return PlaybackManager.Instance; }
        }
    }
}
