using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace SkyJukebox.Data
{
    [Serializable]
    public class Settings
    {
        [NonSerialized]
        private static readonly XmlSerializer MyXs = new XmlSerializer(typeof(Settings));

        [NonSerialized]
        private static string _filePath;

        private Settings()
        {
            DisableAeroGlass = new BoolProperty();
            LoadPlaylistOnStartup = new BoolProperty();
            ShowPlaylistEditorOnStartup = new BoolProperty();
            EnableRecolor = new BoolProperty();
            TextScrollingDelay = new DoubleProperty();
            GuiColor = new ColorProperty();
            ProgressColor = new ColorProperty();
            BgColor = new ColorProperty();
            HeaderFormat = new StringProperty();
            SelectedSkin = new StringProperty();
        }

        private static Settings _instance;
        public static Settings Instance { get { return _instance; } }
        public static void Init(string path)
        {
            _instance = new Settings();
            _filePath = path;
            if (File.Exists(path))
                LoadFromXml();

            _instance.DisableAeroGlass.DefaultValue = false;
            _instance.LoadPlaylistOnStartup.DefaultValue = false;
            _instance.ShowPlaylistEditorOnStartup.DefaultValue = false;
            _instance.EnableRecolor.DefaultValue = false;
            _instance.TextScrollingDelay.DefaultValue = 0.21;
            _instance.GuiColor.DefaultValue = Color.Black;
            _instance.ProgressColor.DefaultValue = Color.FromArgb(127, 31, 199, 15);
            _instance.BgColor.DefaultValue = Color.Transparent;
            _instance.HeaderFormat.DefaultValue = "{1} - {0}";
            _instance.SelectedSkin.DefaultValue = "Default Skin";
        }

        public BoolProperty LoadPlaylistOnStartup { get; set; }
        public string PlaylistToAutoLoad { get; set; }
        public BoolProperty DisableAeroGlass { get; set; }
        public Point LastWindowLocation { get; set; }
        public BoolProperty ShowPlaylistEditorOnStartup { get; set; }
        public StringProperty HeaderFormat { get; set; }
        public DoubleProperty TextScrollingDelay { get; set; }
        public BoolProperty EnableRecolor { get; set; }

        public ColorProperty GuiColor { get; set; }
        public ColorProperty ProgressColor { get; set; }
        public ColorProperty BgColor { get; set; }

        public Guid PlaybackDevice { get; set; }
        public StringProperty SelectedSkin { get; set; }

        private static void LoadFromXml()
        {
            try
            {
                var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var t = (Settings) MyXs.Deserialize(fs);
                fs.Close();
                _instance = t;
            }
            catch
            {
            }
        }
        public static void SaveToXml()
        {
            if (!File.Exists(_filePath)) File.Create(_filePath);
            var fs = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write);
            MyXs.Serialize(fs, _instance);
            fs.Close();
        }
    }
}
