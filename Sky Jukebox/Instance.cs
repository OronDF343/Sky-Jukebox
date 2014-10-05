using System.Drawing;
using SkyJukebox.PluginAPI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SkyJukebox
{
    static class Instance
    {
        public static BackgroundPlayer BgPlayer = new BackgroundPlayer();
        public static Settings Settings;
        public const string SettingsPath = @"settings.xml";
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static List<IPlugin> LoadedPlugins = new List<IPlugin>();
        public static List<ICodec> LoadedCodecs = new List<ICodec>();
        public const string IconsPackUri = "pack://application:,,,/Icons/";
        public static Dictionary<string, Uri> IconUriDictionary = new Dictionary<string, Uri>();
        public static Dictionary<string, Image> IconImageDictionary = new Dictionary<string, Image>();
        public static readonly Dictionary<string, string> CommmandLineArgs = new Dictionary<string, string>();

        static Instance()
        {
            IconUriDictionary.Add("next32", new Uri(IconsPackUri + "next-icon-32.png"));
            IconUriDictionary.Add("play32", new Uri(IconsPackUri + "play-icon-32.png"));
            IconUriDictionary.Add("previous32", new Uri(IconsPackUri + "previous-icon-32.png"));
            IconUriDictionary.Add("stop32", new Uri(IconsPackUri + "stop-icon-32.png"));
            IconUriDictionary.Add("shuffle32off", new Uri(IconsPackUri + "shuffle-icon-32-off.png"));
            IconUriDictionary.Add("shuffle32", new Uri(IconsPackUri + "shuffle-icon-32.png"));
            IconUriDictionary.Add("loop32none", new Uri(IconsPackUri + "repeat-3-icon-32-off.png"));
            IconUriDictionary.Add("loop32single", new Uri(IconsPackUri + "repeat-2-icon-32.png"));
            IconUriDictionary.Add("loop32all", new Uri(IconsPackUri + "repeat-3-icon-32.png"));
            IconUriDictionary.Add("playlist32", new Uri(IconsPackUri + "iconmonstr-play-file-2-icon-32.png"));
            IconUriDictionary.Add("edit32", new Uri(IconsPackUri + "iconmonstr-edit-6-icon-32.png"));
            IconUriDictionary.Add("settings32", new Uri(IconsPackUri + "iconmonstr-gear-5-icon-32.png"));
            IconUriDictionary.Add("color32", new Uri(IconsPackUri + "iconmonstr-paintbrush-7-icon-32.png"));
            IconUriDictionary.Add("minimize32", new Uri(IconsPackUri + "iconmonstr-window-remove-icon-32.png"));
            IconUriDictionary.Add("info32", new Uri(IconsPackUri + "iconmonstr-info-2-icon-32.png"));
            IconUriDictionary.Add("exit32", new Uri(IconsPackUri + "iconmonstr-power-off-icon-32.png"));
        }
    }
}
