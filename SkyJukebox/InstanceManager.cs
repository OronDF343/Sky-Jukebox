using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Win32;
using ShellDll;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox
{
    static class InstanceManager
    {
        public static string ExeDir { get; private set; }
        public static string ExeFilePath { get; private set; }
        public static string UserDataDir { get; private set; }
        public const string SettingsFileName = @"settings.xml";
        public const string SkinsFolderName = @"skins";
        public const string KeyConfigFileName = @"keyconfig.xml";
        public const string ProgId = "SkyJukebox";
        public static SettingsWindow SettingsWindowInstance;
        public static PlaylistEditor PlaylistEditorInstance;
        public static MiniPlayer MiniPlayerInstance;
        public static IEnumerable<ExtensionInfo<IPlugin>> LoadedPlugins;
        public static List<string> CommmandLineArgs;

        static InstanceManager()
        {
            // Find the exe path
            ExeFilePath = Assembly.GetExecutingAssembly().Location;
            ExeDir = ExeFilePath.SubstringRange(0, ExeFilePath.LastIndexOf('\\') + 1);
            // Check if this is an installed copy of Sky Jukebox
            var key = Registry.LocalMachine.OpenSubKey(@"Software\OronDF343\SkyJukebox");
            if (key != null)
            {
                Console.WriteLine("Installation detected");
                var loc = key.GetValue("InstallLocation");
                if (((string)loc).Trim('\"').Equals(ExeDir))
                {
                    Console.WriteLine("Loading from AppData");
                    UserDataDir = PathEx.Combine(new DirectoryInfoEx(KnownFolderIds.LocalAppData).FullName, "SkyJukebox");
                    if (!DirectoryEx.Exists(UserDataDir)) DirectoryEx.CreateDirectory(UserDataDir);
                    return;
                }
            }
            UserDataDir = ExeDir;
        }

        public const string CurrentReleaseTag = "v0.9-alpha4.x";
    }
}
