using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using SkyJukebox.Lib.Xml;
using TagLib.Riff;
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

        private static Settings _instance;
        public static Settings Instance { get { return _instance; } }
        public static void Load(string path)
        {
            _instance = new Settings();
            _filePath = path;
            if (File.Exists(path))
                LoadFromXml();

            _instance.DisableAeroGlass.DefaultValue = false;
            _instance.RestoreLocation.DefaultValue = false;
            _instance.LoadPlaylistOnStartup.DefaultValue = false;
            _instance.ShowPlaylistEditorOnStartup.DefaultValue = false;
            _instance.EnableRecolor.DefaultValue = false;
            _instance.TextScrollingDelay.DefaultValue = 0.21;
            _instance.GuiColor.DefaultValue = Color.Black;
            _instance.ProgressColor.DefaultValue = Color.FromArgb(127, 31, 199, 15);
            _instance.BgColor.DefaultValue = Color.Transparent;
            _instance.HeaderFormat.DefaultValue = "{1} - {0}";
            _instance.SelectedSkin.DefaultValue = "Default Skin";
            #region PlaylistEditorColumnsVisibility values
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("FileName"))
                _instance.PlaylistEditorColumnsVisibility.Add("FileName", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Title"))
                _instance.PlaylistEditorColumnsVisibility.Add("Title", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Performers"))
                _instance.PlaylistEditorColumnsVisibility.Add("Performers", false);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("AlbumArtists"))
                _instance.PlaylistEditorColumnsVisibility.Add("AlbumArtists", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Album"))
                _instance.PlaylistEditorColumnsVisibility.Add("Album", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("TrackNumber"))
                _instance.PlaylistEditorColumnsVisibility.Add("TrackNumber", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Genre"))
                _instance.PlaylistEditorColumnsVisibility.Add("Genre", false);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Year"))
                _instance.PlaylistEditorColumnsVisibility.Add("Year", false);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Duration"))
                _instance.PlaylistEditorColumnsVisibility.Add("Duration", true);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Codec"))
                _instance.PlaylistEditorColumnsVisibility.Add("Codec", false);
            if (!_instance.PlaylistEditorColumnsVisibility.ContainsKey("Bitrate"))
                _instance.PlaylistEditorColumnsVisibility.Add("Bitrate", false);
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
                _instance = t;
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
            MyXs.Serialize(fs, _instance);
            fs.Close();
        }
    }
}
