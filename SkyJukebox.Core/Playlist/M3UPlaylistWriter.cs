using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib.Xml;

namespace SkyJukebox.Core.Playlist
{
    public class M3UPlaylistWriter : IPlaylistWriter
    {
        static M3UPlaylistWriter()
        {
            if (!SettingsManager.Instance.ContainsKey("M3U8:UseRelativePath"))
                SettingsManager.Instance.Add("M3U8:UseRelativePath", new BoolProperty(true));
            else
                SettingsManager.Instance["M3U8:UseRelativePath"].DefaultValue = true;
        }

        public static bool UseRelativePath
        {
            get { return (bool)SettingsManager.Instance["M3U8:UseRelativePath"].Value; }
            set { SettingsManager.Instance["M3U8:UseRelativePath"].Value = value; }
        }

        public string Id { get { return "m3u8"; } }
        public string FormatName { get { return "M3U8 Playlist"; } }
        public IEnumerable<string> FormatExtensions { get { return new[] { "m3u8" }; } }
        public bool WritePlaylist(string path, IEnumerable<string> entries)
        {
            try
            {
                File.WriteAllLines(path, entries.Select(s => UseRelativePath ? FileUtils.MakeRelativePath(path, s) : s), Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
