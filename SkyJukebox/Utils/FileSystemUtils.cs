using System;
using System.IO;
using System.Windows;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Lib;

namespace SkyJukebox.Utils
{
    public static class FileSystemUtils
    {
        public static void AddFolderQuery(DirectoryInfoEx di)
        {
            var dr = MessageBoxResult.No;
            if (di.HasSubFolder)
                dr = MessageBox.Show("Import subfolders?", "Add Folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            var r = dr == MessageBoxResult.Yes;
            if (r || dr == MessageBoxResult.No)
                FileUtils.AddFolder(di, r, DefaultLoadErrorCallback);
        }

        public static readonly Action<Exception, string> DefaultLoadErrorCallback =
            (ex, s) =>
            MessageBox.Show("Error loading file \"" + s + "\": " + ex.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);

        public static void AddFolderQuery(string path)
        {
            AddFolderQuery(new DirectoryInfoEx(path));
        }

        public static bool LoadFileFromClArgs()
        {
            InstanceManager.CommmandLineArgs.RemoveAt(0);
            if (InstanceManager.CommmandLineArgs.Count == 0) return false;
            var file = InstanceManager.CommmandLineArgs.Find(s => !s.StartsWith("--"));
            if (file == default(string)) return false;

            var fsi = FileSystemInfoEx.FromString(file);

            if (fsi.IsFolder)
                AddFolderQuery(fsi as DirectoryInfoEx);
            else if (!fsi.Exists)
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
                        PlaybackManager.Instance.Playlist.AddRange(file, DefaultLoadErrorCallback);
                    else
                        return false;
                }
                else if (PlaybackManager.Instance.HasSupportingPlayer(ext))
                    PlaybackManager.Instance.Playlist.Add(MusicInfo.Create(file, DefaultLoadErrorCallback));
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
