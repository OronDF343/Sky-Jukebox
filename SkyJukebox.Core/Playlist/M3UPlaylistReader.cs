using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkyJukebox.Api.Playlist;

namespace SkyJukebox.Core.Playlist
{
    public class M3UPlaylistReader : IPlaylistReader
    {
        public string Id { get { return "m3u*"; } }
        public string FormatName { get { return "M3U / M3U8 Playlist"; } }
        public IEnumerable<string> FormatExtensions { get { return new[] { "m3u", "m3u8" }; } }
        public bool GetPlaylistFiles(string path, out IEnumerable<string> entries)
        {
            try
            {
                var dir = new FileInfoEx(path).DirectoryName.TrimEnd('\\');
                entries = new List<string>(from f in FileEx.ReadAllLines(path)
                                           where !string.IsNullOrWhiteSpace(f) && !f.StartsWith("#EXT")
                                           select f[1] == ':' ? f : (dir + "\\" + f));
                return true;
            }
            catch
            {
                entries = null;
                return false;
            }
        }
    }
}
