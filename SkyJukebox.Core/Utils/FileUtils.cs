using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Playlist;
using SkyJukebox.Lib;

namespace SkyJukebox.Core.Utils
{
    public static class FileUtils
    {
        public static async Task<bool> AddFolder(DirectoryInfoEx dir, bool subfolders, Action<Exception, string> errorCallback)
        {
            var arr = Task.Run(() =>
            {
                IEnumerable<IMusicInfo> stuff;
                if (subfolders)
                    stuff = from f in dir.EnumerateFilesEx()
                            where PlaybackManager.Instance.HasSupportingPlayer(f.Name.GetExt())
                            select MusicInfo.Create(f, errorCallback);
                else
                    stuff = from f in dir.GetFiles()
                            where PlaybackManager.Instance.HasSupportingPlayer(f.Name.GetExt())
                            select MusicInfo.Create(f, errorCallback);
                return stuff.ToArray();
            });
            PlaybackManager.Instance.Playlist.AddRange(await arr);
            return true;
        }

        public static bool SavePlaylist(string file)
        {
            // Cast might be important because of the ShuffledIndex.
            return PlaylistDataManager.Instance.WritePlaylist(file, (PlaybackManager.Instance.Playlist as IEnumerable<IMusicInfo>).Select(m => m.FilePath));
        }
    }
}
