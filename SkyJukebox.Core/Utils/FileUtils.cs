using System;
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
    }
}
