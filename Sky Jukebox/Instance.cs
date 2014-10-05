﻿using SkyJukebox.PluginAPI;
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
        public static Dictionary<string, string> IconDictioanry = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> CommmandLineArgs = new Dictionary<string, string>();
    }
}
