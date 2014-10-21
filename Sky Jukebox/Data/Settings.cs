﻿using System;
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
            // For serializing. Do not use.
            DisableAeroGlass = new BoolProperty(false);
            LoadPlaylistOnStartup = new BoolProperty(false);
            ShowPlaylistEditorOnStartup = new BoolProperty(false);
            EnableRecolor = new BoolProperty(false);
            GuiColor = new ColorProperty(Color.Black);
            ProgressColor = new ColorProperty(Color.FromArgb(127, 31, 199, 15));
            BgColor = new ColorProperty(Color.Transparent);
        }

        private static Settings _instance;
        public static Settings Instance { get { return _instance; } }
        public static void Init(string path)
        {
            _instance = new Settings();
            _filePath = path;
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
        public BoolProperty EnableRecolor { get; set; }

        public ColorProperty GuiColor { get; set; }
        public ColorProperty ProgressColor { get; set; }
        public ColorProperty BgColor { get; set; }

        public Guid PlaybackDevice { get; set; }
        public string SelectedSkin { get; set; }

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
