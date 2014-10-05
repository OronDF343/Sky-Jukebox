﻿using System;
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
        [XmlIgnore]
        public string FilePath { get; set; }

        public Settings()
        {
            // Dummy for serializing. Do not use.
            DisableAeroGlass = new BoolProperty(false);
            LoadPlaylistOnStartup = new BoolProperty(false);
            ShowPlaylistEditorOnStartup = new BoolProperty(false);
        }

        public Settings(string path)
            : this()
        {
            _myXs = new XmlSerializer(typeof(Settings));
            FilePath = path;
            if (File.Exists(path))
                LoadFromXml();
        }

        public BoolProperty LoadPlaylistOnStartup { get; set; }
        public string PlaylistToAutoLoad { get; set; }
        public BoolProperty DisableAeroGlass { get; set; }
        public Point LastWindowLocation { get; set; }
        public BoolProperty ShowPlaylistEditorOnStartup { get; set; }
        public string HeaderFormat { get; set; }
        public double TextScrollingDelay { get; set; }

        private void LoadFromXml()
        {
            try
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var t = (Settings)_myXs.Deserialize(fs);
                    LoadPlaylistOnStartup.Value = t.LoadPlaylistOnStartup.Value;
                    PlaylistToAutoLoad = t.PlaylistToAutoLoad;
                    DisableAeroGlass.Value = t.DisableAeroGlass.Value;
                    LastWindowLocation = t.LastWindowLocation;
                    ShowPlaylistEditorOnStartup.Value = t.ShowPlaylistEditorOnStartup.Value;
                    HeaderFormat = t.HeaderFormat;
                    TextScrollingDelay = t.TextScrollingDelay;
                }
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException)) throw;
                File.Create(FilePath);
                LoadFromXml();
            }
        }
        public void SaveToXml()
        {
            if (!File.Exists(FilePath)) File.Create(FilePath);
            using (var fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.Write))
            {
                _myXs.Serialize(fs, this);
            }
        }
    }
}
