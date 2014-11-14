using System.Reflection;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
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

        public static bool ClosePlaylistQuery()
        {
            if (PlaybackManager.Instance.Playlist.Count <= 0) return true;
            var dr = MessageBox.Show("Save current playlist?", "Playlist", MessageBoxButton.YesNoCancel,
                                     MessageBoxImage.Question, MessageBoxResult.Cancel);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    {
                        var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
                        if (sfd.ShowDialog() == true)
                            StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, sfd.FileName, true);
                        else
                            return false;
                    }
                    break;
                case MessageBoxResult.Cancel:
                    return false;
            }

            return true;
        }
    }
}
