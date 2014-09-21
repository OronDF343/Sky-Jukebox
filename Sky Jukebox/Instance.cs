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
        public static string SettingsPath = @"settings.xml";
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static List<IPlugin> LoadedPlugins = new List<IPlugin>();
        public static List<ICodec> LoadedCodecs = new List<ICodec>();
    }
}
