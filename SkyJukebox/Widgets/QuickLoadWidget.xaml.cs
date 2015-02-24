using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Xml;

namespace SkyJukebox.Widgets
{
    /// <summary>
    /// Interaction logic for QuickLoadWidget.xaml
    /// </summary>
    public partial class QuickLoadWidget : INotifyPropertyChanged
    {
        public QuickLoadWidget()
        {
            DisableAeroGlass = (bool)SettingsManager.Instance["DisableAeroGlass"].Value;
            InitializeComponent();
            IconManagerInstance.CollectionChanged += (sender, args) => OnPropertyChanged("IconManagerInstance");
            AddFilesButton.Click += (sender, e) => 
            {
                DoFocusChange();
                InstanceManager.Instance.PlaylistEditorInstance.AddFiles_Click(sender, e);
            };
            AddFolderButton.Click += (sender, e) =>
            {
                DoFocusChange();
                InstanceManager.Instance.PlaylistEditorInstance.AddFolder_Click(sender, e);
            };
            OpenPlaylistButton.Click += (sender, e) =>
            {
                DoFocusChange();
                InstanceManager.Instance.PlaylistEditorInstance.OpenPlaylist_Click(sender, e);
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

        public IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }

        public SettingsManager SettingsInstance { get { return SettingsManager.Instance; } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
