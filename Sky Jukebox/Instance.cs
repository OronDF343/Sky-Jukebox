using System.Reflection;
using System.Collections.Generic;
using SkyJukebox.CoreApi.Contracts;
using SkyJukebox.CoreApi.Utils;

namespace SkyJukebox
{
    static class InstanceManager
    {
        public static string ExePath { get; private set; }
        public const string SettingsPath = @"settings.xml";
        public const string SkinsPath = @"skins";
        public const string KeyConfigPath = @"keyconfig.xml";
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static IEnumerable<IPlugin> LoadedPlugins = new List<IPlugin>();
        public static string[] CommmandLineArgs;

        static InstanceManager()
        {
            // Find the exe path
            var epath = Assembly.GetExecutingAssembly().Location;
            ExePath = epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
        }
    }
}
