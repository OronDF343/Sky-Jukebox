using System.IO;
using System.Linq;
using System.Windows;
using SkyJukebox.Core.Playback;
using SkyJukebox.Lib;

namespace SkyJukebox.Utils
{
    public static class DirUtils
    {
        public static void AddFolder(string path)
        {
            var dr = MessageBoxResult.No;
            var di = new DirectoryInfo(path);
            if (di.GetDirectories().Length > 0)
                dr = MessageBox.Show("Import subfolders?", "Add Folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    PlaybackManager.Instance.Playlist.AddRange(from f in Lib.Utils.GetFiles(path)
                                                               where PlaybackManager.Instance.HasSupportingPlayer(f.GetExt())
                                                               select new MusicInfo(f));
                    break;
                case MessageBoxResult.No:
                    PlaybackManager.Instance.Playlist.AddRange(from f in di.GetFiles()
                                                               where PlaybackManager.Instance.HasSupportingPlayer(f.Name.GetExt())
                                                               select new MusicInfo(f.FullName));
                    break;
            }
        }

        public static bool LoadFileFromClArgs()
        {
            InstanceManager.CommmandLineArgs.RemoveAt(0);
            if (InstanceManager.CommmandLineArgs.Count == 0) return false;
            var file = InstanceManager.CommmandLineArgs.Find(s => !s.StartsWith("--"));
            if (file == default(string)) return false;

            if (Directory.Exists(file))
            {
                AddFolder(file);
            }
            else if (!File.Exists(file))
            {
                MessageBox.Show("Invalid command line argument or file/directory not found: " + file,
                                "Non-critical error, everything is ok!", MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
                return false;
            }
            else
            {
                var ext = file.GetExt();
                if (ext.StartsWith("m3u")) // TODO: when other playlist format support is added, update this!
                {
                    if (InstanceManager.PlaylistEditorInstance.ClosePlaylistQuery())
                        PlaybackManager.Instance.Playlist = new Playlist(file);
                    else
                        return false;
                }
                else if (PlaybackManager.Instance.HasSupportingPlayer(ext))
                    PlaybackManager.Instance.Playlist.Add(file);
                else
                {
                    MessageBox.Show("Unsupported file type: " + ext, "Non-critical error, everything is ok!",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }
            // by now we have determined that stuff was successfully added
            if (PlaybackManager.Instance.CurrentState != PlaybackManager.PlaybackStates.Playing)
                PlaybackManager.Instance.PlayPauseResume();
            return true;
        }
    }
}
