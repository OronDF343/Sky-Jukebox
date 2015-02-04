using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Lib;

namespace SkyJukebox.Core.Utils
{
    public static class FileUtils
    {
        public static async void AddFolder(DirectoryInfoEx dir, bool subfolders)
        {
            var arr = Task.Run(() =>
            {
                IEnumerable<IMusicInfo> stuff;
                if (subfolders)
                    stuff = from f in dir.EnumerateFilesEx()
                            where PlaybackManager.Instance.HasSupportingPlayer(f.Name.GetExt())
                            select new MusicInfo(f);
                else
                    stuff = from f in dir.GetFiles()
                            where PlaybackManager.Instance.HasSupportingPlayer(f.Name.GetExt())
                            select new MusicInfo(f);
                return stuff.ToArray();
            });
            PlaybackManager.Instance.Playlist.AddRange(await arr);
        }
    }
}
