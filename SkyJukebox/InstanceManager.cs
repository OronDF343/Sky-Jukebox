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
    public class InstanceManager : IInstanceManager
    {
        private static InstanceManager _instance;
        public static InstanceManager Instance { get { return _instance ?? (_instance = new InstanceManager()); } }

        public string ExeDir { get; private set; }
        public string ExeFilePath { get; private set; }
        public string UserDataDir { get; private set; }
        public string SettingsFileName { get { return @"settings.xml"; } }
        public string SettingsFilePath { get { return PathEx.Combine(UserDataDir, SettingsFileName); } }
        public string SkinsFolderName { get { return @"skins"; } }
        public string SkinsFolderPath { get { return PathEx.Combine(UserDataDir, SkinsFolderName); } }
        public string KeyConfigFileName { get { return @"keyconfig.xml"; } }
        public string KeyConfigFilePath { get { return PathEx.Combine(UserDataDir, KeyConfigFileName); } }
        public string ProgId { get { return "SkyJukebox"; } }
        public IEnumerable<ExtensionInfo<IExtension>> LoadedExtensions { get; internal set; }
        public List<string> CommmandLineArgs { get; internal set; }
        public string CurrentReleaseTag { get { return "v0.9-alpha4.x"; } }
        // UI only:
        public SettingsWindow SettingsWindowInstance;
        public PlaylistEditor PlaylistEditorInstance;
        public MiniPlayer MiniPlayerInstance;

        private InstanceManager()
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
    }
}
