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

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
