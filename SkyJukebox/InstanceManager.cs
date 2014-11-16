﻿using System.Reflection;
using System.Collections.Generic;
using SkyJukebox.Api;
using SkyJukebox.Lib;

namespace SkyJukebox
{
    static class InstanceManager
    {
        public static string ExeDir { get; private set; }
        public static string ExeFilePath { get; private set; }
        public const string SettingsPath = @"settings.xml";
        public const string SkinsPath = @"skins";
        public const string KeyConfigPath = @"keyconfig.xml";
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static IEnumerable<IPlugin> LoadedPlugins;
        public static List<string> CommmandLineArgs;

        static InstanceManager()
        {
            // Find the exe path
            ExeFilePath = Assembly.GetExecutingAssembly().Location;
            ExeDir = ExeFilePath.SubstringRange(0, ExeFilePath.LastIndexOf('\\') + 1);
        }
    }
}
