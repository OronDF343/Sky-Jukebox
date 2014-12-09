using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using SkyJukebox.Lib.Xml;
using File = System.IO.File;

namespace SkyJukebox.Core.Xml
{
    [Serializable]
    public class Settings
    {
        private static readonly XmlSerializer MyXs = new XmlSerializer(typeof(Settings));

        private static string _filePath;

        private Settings()
        {
            DisableAeroGlass = new BoolProperty();
            RestoreLocation = new BoolProperty();
            LoadPlaylistOnStartup = new BoolProperty();
            ShowPlaylistEditorOnStartup = new BoolProperty();
            EnableRecolor = new BoolProperty();
            TextScrollingDelay = new DoubleProperty();
            GuiColor = new ColorProperty();
            ProgressColor = new ColorProperty();
            BgColor = new ColorProperty();
            HeaderFormat = new StringProperty();
            SelectedSkin = new StringProperty();
            PlaylistEditorColumnsVisibility = new SerializableDictionary<string, bool>();
        }

        public static Settings Instance { get; private set; }

        public static void Load(string path)
        {
            Instance = new Settings();
            _filePath = path;
            if (File.Exists(path))
                LoadFromXml();

            Instance.DisableAeroGlass.DefaultValue = false;
            Instance.RestoreLocation.DefaultValue = false;
            Instance.LoadPlaylistOnStartup.DefaultValue = false;
            Instance.ShowPlaylistEditorOnStartup.DefaultValue = false;
            Instance.EnableRecolor.DefaultValue = false;
            Instance.TextScrollingDelay.DefaultValue = 0.21;
            Instance.GuiColor.DefaultValue = Color.Black;
            Instance.ProgressColor.DefaultValue = Color.FromArgb(127, 31, 199, 15);
            Instance.BgColor.DefaultValue = Color.Transparent;
            Instance.HeaderFormat.DefaultValue = "$PJ($AJ) - $TI($FN)";
            Instance.SelectedSkin.DefaultValue = "Default Skin";
            #region PlaylistEditorColumnsVisibility values
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("FileName"))
                Instance.PlaylistEditorColumnsVisibility.Add("FileName", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Title"))
                Instance.PlaylistEditorColumnsVisibility.Add("Title", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Performers"))
                Instance.PlaylistEditorColumnsVisibility.Add("Performers", false);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("AlbumArtists"))
                Instance.PlaylistEditorColumnsVisibility.Add("AlbumArtists", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Album"))
                Instance.PlaylistEditorColumnsVisibility.Add("Album", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("TrackNumber"))
                Instance.PlaylistEditorColumnsVisibility.Add("TrackNumber", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Genre"))
                Instance.PlaylistEditorColumnsVisibility.Add("Genre", false);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Year"))
                Instance.PlaylistEditorColumnsVisibility.Add("Year", false);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Duration"))
                Instance.PlaylistEditorColumnsVisibility.Add("Duration", true);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Codec"))
                Instance.PlaylistEditorColumnsVisibility.Add("Codec", false);
            if (!Instance.PlaylistEditorColumnsVisibility.ContainsKey("Bitrate"))
                Instance.PlaylistEditorColumnsVisibility.Add("Bitrate", false);
            #endregion
        }

        public static void DiscardChanges()
        {
            // TODO: Cache the settings so no Disk I/O is needed. BeginEdit() -> Save()/Discard() -> EndEdit().
            Load(_filePath);
        }

        public BoolProperty LoadPlaylistOnStartup { get; set; }
        public string PlaylistToAutoLoad { get; set; }
        public BoolProperty DisableAeroGlass { get; set; }
        public BoolProperty RestoreLocation { get; set; }
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

        public SerializableDictionary<string, bool> PlaylistEditorColumnsVisibility { get; set; } 

        private static void LoadFromXml()
        {
            try
            {
                var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var t = (Settings) MyXs.Deserialize(fs);
                fs.Close();
                Instance = t;
            }
            catch
            {
            }
        }
        public static void SaveToXml()
        {
            if (_filePath == null) return;
            if (!File.Exists(_filePath))
            {
                // work around bug with File.Create()
                var cs = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                cs.Close();
            }
            var fs = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write);
            MyXs.Serialize(fs, Instance);
            fs.Close();
        }
    }
}
