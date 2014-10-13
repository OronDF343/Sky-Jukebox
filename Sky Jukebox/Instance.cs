using System.Drawing;
using System.Reflection;
using SkyJukebox.PluginAPI;
using System;
using System.Collections.Generic;

namespace SkyJukebox
{
    static class Instance
    {
        public static string ExePath { get; private set; }
        public const string SettingsPath = @"settings.xml";
        public const string SkinsPath = @"skins";
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static IEnumerable<IPlugin> LoadedPlugins = new List<IPlugin>();
        public static readonly Dictionary<string, string> CommmandLineArgs = new Dictionary<string, string>();

        static Instance()
        {
            // Find the exe path
            var epath = Assembly.GetExecutingAssembly().Location;
            ExePath = epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
        }
    }
}
