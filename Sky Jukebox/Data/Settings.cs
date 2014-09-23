using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SkyJukebox
{
    [Serializable]
    public class Settings
    {
        [NonSerialized]
        private readonly XmlSerializer _myXs;
        [NonSerialized]
        public readonly string FilePath;

        public Settings()
        {
            // Dummy for serializing. Do not use.
        }

        public Settings(string path)
        {
            _myXs = new XmlSerializer(typeof(Settings));
            FilePath = path;
            if (File.Exists(path))
                LoadFromXml();
        }

        public bool LoadPlaylistOnStartup { get; set; }
        public string PlaylistToAutoLoad { get; set; }
        public bool DisableAeroGlassEffect { get; set; }
        public Point LastWindowLocation { get; set; }
        public bool ShowPlaylistEditorOnStartup { get; set; }
        public string HeaderFormat { get; set; }
        public double TextScrollingSpeed { get; set; }

        public void LoadFromXml()
        {
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                var t = (Settings)_myXs.Deserialize(fs);
                fs.Close();
                LoadPlaylistOnStartup = t.LoadPlaylistOnStartup;
                PlaylistToAutoLoad = t.PlaylistToAutoLoad;
                DisableAeroGlassEffect = t.DisableAeroGlassEffect;
                LastWindowLocation = t.LastWindowLocation;
                ShowPlaylistEditorOnStartup = t.ShowPlaylistEditorOnStartup;
                HeaderFormat = t.HeaderFormat;
                TextScrollingSpeed = t.TextScrollingSpeed;
            }
        }
        public void SaveToXml()
        {
            using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                _myXs.Serialize(fs, this);
                fs.Close();
            }
        }
    }
}
