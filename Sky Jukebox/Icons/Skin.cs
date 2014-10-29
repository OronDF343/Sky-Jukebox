using SkyJukebox.CoreApi.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SkyJukebox.Icons
{
    [Serializable]
    public class Skin
    {
        public string Name { get; set; }

        [XmlIgnore]
        public bool IsEmbedded { get; set; }

        public bool AutoDisableRecoloring { get; set; }
        public ColorProperty DefaultIconColor { get; set; }
        public ColorProperty DefaultBgColor { get; set; }
        public ColorProperty DefaultProgressColor { get; set; }

        [XmlArray("IconEntries")]
        [XmlArrayItem("IconEntry")]
        public List<IconEntry> IconEntries { get; set; }

        [Serializable]
        public class IconEntry
        {
            public IconEntry() { }
            public IconEntry(string key, string path)
            {
                Key = key;
                Path = path;
            }
            [XmlAttribute]
            public string Key { get; set; }
            [XmlAttribute]
            public string Path { get; set; }
        }

        [NonSerialized]
        private const string IconsPackUri = "pack://application:,,,/IconFiles/";

        [NonSerialized]
        public static readonly Skin DefaultSkin = new Skin
        {
            Name = "Default Skin",
            IsEmbedded = true,
            AutoDisableRecoloring = false,
            IconEntries = new List<IconEntry>
            {
                // 32px
                new IconEntry("next32", IconsPackUri + "next-icon-32.png"),
                new IconEntry("play32", IconsPackUri + "play-icon-32.png"),
                new IconEntry("pause32", IconsPackUri + "pause-icon-32.png"),
                new IconEntry("previous32", IconsPackUri + "previous-icon-32.png"),
                new IconEntry("stop32", IconsPackUri + "stop-icon-32.png"),
                new IconEntry("shuffle32off", IconsPackUri + "shuffle-icon-32-off.png"),
                new IconEntry("shuffle32", IconsPackUri + "shuffle-icon-32.png"),
                new IconEntry("loop32none", IconsPackUri + "repeat-3-icon-32-off.png"),
                new IconEntry("loop32single", IconsPackUri + "repeat-2-icon-32.png"),
                new IconEntry("loop32all", IconsPackUri + "repeat-3-icon-32.png"),
                new IconEntry("playlist32", IconsPackUri + "iconmonstr-play-file-2-icon-32.png"),
                new IconEntry("edit32", IconsPackUri + "iconmonstr-edit-6-icon-32.png"),
                new IconEntry("settings32", IconsPackUri + "iconmonstr-gear-5-icon-32.png"),
                new IconEntry("color32", IconsPackUri + "iconmonstr-paintbrush-7-icon-32.png"),
                new IconEntry("minimize32", IconsPackUri + "iconmonstr-window-remove-icon-32.png"),
                new IconEntry("info32", IconsPackUri + "iconmonstr-info-2-icon-32.png"),
                new IconEntry("exit32", IconsPackUri + "iconmonstr-power-off-icon-32.png"),
                // 16px
                new IconEntry("add16file", IconsPackUri + "iconmonstr-add-file-2-icon-16.png"),
                new IconEntry("add16folder", IconsPackUri + "iconmonstr-add-folder-2-icon-16.png"),
                new IconEntry("remove16file", IconsPackUri + "iconmonstr-remove-file-2-icon-16.png"),
                new IconEntry("remove16all", IconsPackUri + "iconmonstr-x-mark-icon-16.png"),
                new IconEntry("move16top", IconsPackUri + "iconmonstr-sort-5-icon-16.png"),
                new IconEntry("move16up", IconsPackUri + "iconmonstr-sort-7-icon-16.png"),
                new IconEntry("move16down", IconsPackUri + "iconmonstr-sort-8-icon-16.png"),
                new IconEntry("move16bottom", IconsPackUri + "iconmonstr-sort-6-icon-16.png"),
                new IconEntry("playlist16", IconsPackUri + "iconmonstr-play-file-2-icon-16.png"),
                new IconEntry("save16", IconsPackUri + "iconmonstr-save-icon-16.png"),
                new IconEntry("save16as", IconsPackUri + "iconmonstr-save-5-icon-16.png")
            }
        };
    }
}
