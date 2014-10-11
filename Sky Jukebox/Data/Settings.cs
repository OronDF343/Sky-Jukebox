using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SkyJukebox.Data
{
    [Serializable]
    public class Settings
    {
        [NonSerialized]
        private readonly XmlSerializer _myXs;

        [NonSerialized]
        private readonly string _filePath;

        private Settings()
        {
            // For serializing. Do not use.
            DisableAeroGlass = new BoolProperty(false);
            LoadPlaylistOnStartup = new BoolProperty(false);
            ShowPlaylistEditorOnStartup = new BoolProperty(false);
        }

        private Settings(string path)
            : this()
        {
            _myXs = new XmlSerializer(typeof(Settings));
            _filePath = path;
            if (File.Exists(path))
                LoadFromXml();
        }

        private static Settings _instance;
        public static Settings Instance { get { return _instance; } }
        public static void Init(string path)
        {
            _instance = new Settings(path);
        }

        public BoolProperty LoadPlaylistOnStartup { get; set; }
        public string PlaylistToAutoLoad { get; set; }
        public BoolProperty DisableAeroGlass { get; set; }
        public Point LastWindowLocation { get; set; }
        public BoolProperty ShowPlaylistEditorOnStartup { get; set; }
        public string HeaderFormat { get; set; }
        public double TextScrollingDelay { get; set; }
        public Color GuiColor { get; set; }
        public Guid PlaybackDevice { get; set; }

        private void LoadFromXml()
        {
            try
            {
                var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var t = (Settings) _myXs.Deserialize(fs);
                fs.Close();
                LoadPlaylistOnStartup.Value = t.LoadPlaylistOnStartup.Value;
                PlaylistToAutoLoad = t.PlaylistToAutoLoad;
                DisableAeroGlass.Value = t.DisableAeroGlass.Value;
                LastWindowLocation = t.LastWindowLocation;
                ShowPlaylistEditorOnStartup.Value = t.ShowPlaylistEditorOnStartup.Value;
                HeaderFormat = t.HeaderFormat;
                TextScrollingDelay = t.TextScrollingDelay;
            }
            catch
            {
            }
        }
        public void SaveToXml()
        {
            if (!File.Exists(_filePath)) File.Create(_filePath);
            var fs = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write);
            _myXs.Serialize(fs, this);
            fs.Close();
        }
    }
}
