using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SkyJukebox.Lib.Xml;

namespace SkyJukebox.Core.Icons
{
    [Serializable]
    public class Skin
    {
        public string Name { get; set; }

        [XmlIgnore]
        public bool IsEmbedded { get; set; }

        // the following 4 properties aren't implemented yet:
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
            public IconEntry(string key, string path, bool allowRecolor = true)
            {
                Key = key;
                Path = path;
                AllowRecolor = allowRecolor;
            }
            [XmlAttribute]
            public string Key { get; set; }
            [XmlAttribute]
            public string Path { get; set; }
            [XmlAttribute]
            public bool AllowRecolor { get; set; }
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
                new IconEntry("volume32", IconsPackUri + "iconmonstr-audio-4-icon-32.png"),
                new IconEntry("mute32", IconsPackUri + "iconmonstr-audio-5-icon-32.png"),
                new IconEntry("add32file", IconsPackUri + "iconmonstr-add-file-2-icon-32.png"),
                new IconEntry("add32folder", IconsPackUri + "iconmonstr-add-folder-2-icon-32.png"),
                new IconEntry("playlist32", IconsPackUri + "iconmonstr-play-file-2-icon-32.png"),
                new IconEntry("edit32", IconsPackUri + "iconmonstr-edit-6-icon-32.png"),
                new IconEntry("settings32", IconsPackUri + "iconmonstr-gear-5-icon-32.png"),
                new IconEntry("color32", IconsPackUri + "iconmonstr-paintbrush-7-icon-32.png"),
                new IconEntry("plugin32", IconsPackUri + "iconmonstr-brick-2-icon-32.png"),
                new IconEntry("minimize32", IconsPackUri + "iconmonstr-window-remove-icon-32.png"),
                new IconEntry("info32", IconsPackUri + "iconmonstr-info-2-icon-32.png"),
                new IconEntry("exit32", IconsPackUri + "iconmonstr-power-off-icon-32.png"),
                // 24px
                new IconEntry("new24", IconsPackUri + "iconmonstr-blank-file-2-icon-24.png", false),
                new IconEntry("open24", IconsPackUri + "iconmonstr-folder-2-icon-24.png", false),
                new IconEntry("save24", IconsPackUri + "iconmonstr-save-icon-24.png", false),
                new IconEntry("save24as", IconsPackUri + "iconmonstr-save-5-icon-24.png", false),
                new IconEntry("add24file", IconsPackUri + "iconmonstr-add-file-2-icon-24.png", false),
                new IconEntry("add24folder", IconsPackUri + "iconmonstr-add-folder-2-icon-24.png", false),
                new IconEntry("playlist24", IconsPackUri + "iconmonstr-play-file-2-icon-24.png", false),
                new IconEntry("remove24file", IconsPackUri + "iconmonstr-remove-file-2-icon-24.png", false),
                new IconEntry("remove24all", IconsPackUri + "iconmonstr-x-mark-icon-24.png", false),
                new IconEntry("move24top", IconsPackUri + "iconmonstr-arrow-31-icon-24-u.png", false),
                new IconEntry("move24up", IconsPackUri + "iconmonstr-arrow-25-icon-24-u.png", false),
                new IconEntry("move24down", IconsPackUri + "iconmonstr-arrow-25-icon-24-d.png", false),
                new IconEntry("move24bottom", IconsPackUri + "iconmonstr-arrow-31-icon-24-d.png", false),
                new IconEntry("sort24", IconsPackUri + "iconmonstr-sort-14-icon-24.png", false),
                new IconEntry("sort24reverse", IconsPackUri + "iconmonstr-sort-16-icon-24.png", false),
                new IconEntry("next24", IconsPackUri + "next-icon-24.png", false),
                new IconEntry("play24", IconsPackUri + "play-icon-24.png", false),
                new IconEntry("pause24", IconsPackUri + "pause-icon-24.png", false),
                new IconEntry("previous24", IconsPackUri + "previous-icon-24.png", false),
                new IconEntry("stop24", IconsPackUri + "stop-icon-24.png", false),
                new IconEntry("shuffle24off", IconsPackUri + "shuffle-icon-24-off.png", false),
                new IconEntry("shuffle24", IconsPackUri + "shuffle-icon-24.png", false),
                new IconEntry("loop24none", IconsPackUri + "repeat-3-icon-24-off.png", false),
                new IconEntry("loop24single", IconsPackUri + "repeat-2-icon-24.png", false),
                new IconEntry("loop24all", IconsPackUri + "repeat-3-icon-24.png", false),
                new IconEntry("volume24", IconsPackUri + "iconmonstr-audio-4-icon-24.png", false),
                new IconEntry("mute24", IconsPackUri + "iconmonstr-audio-5-icon-24.png", false),
                new IconEntry("add24right", IconsPackUri + "iconmonstr-arrow-31-icon-24.png", false),
                new IconEntry("refresh24", IconsPackUri + "iconmonstr-refresh-3-icon-24.png", false),
            }
        };
    }
}
