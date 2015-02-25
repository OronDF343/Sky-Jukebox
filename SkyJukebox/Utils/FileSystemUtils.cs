using System;
using System.IO;
using System.Windows;
using SkyJukebox.Api.Playback;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Playlist;
using SkyJukebox.Core.Utils;
using SkyJukebox.Lib;

namespace SkyJukebox.Utils
{
    public static class FileSystemUtils
    {
        public static async void AddFolderQuery(DirectoryInfoEx di)
        {
            var dr = MessageBoxResult.No;
            if (di.HasSubFolder)
                dr = MessageBox.Show("Import subfolders?", "Add Folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            var r = dr == MessageBoxResult.Yes;
            if (r || dr == MessageBoxResult.No)
                await FileUtils.AddFolder(di, r, DefaultLoadErrorCallback);
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
            InstanceManager.Instance.CommmandLineArgs.RemoveAt(0);
            if (InstanceManager.Instance.CommmandLineArgs.Count == 0) return false;
            var file = InstanceManager.Instance.CommmandLineArgs.Find(s => !s.StartsWith("--"));
            if (file == default(string)) return false;
            var addOnly = InstanceManager.Instance.CommmandLineArgs.Find(s => s.ToLowerInvariant() == "--add") != default(string);

            var fsi = FileSystemInfoEx.FromString(file);

            if (fsi.IsFolder)
            {
                var l = PlaybackManager.Instance.Playlist.Count;
                AddFolderQuery(fsi as DirectoryInfoEx);
                if (l < PlaybackManager.Instance.Playlist.Count && !addOnly) PlaybackManager.Instance.NowPlayingId = l;
            }
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
                if (PlaylistDataManager.Instance.HasReader(ext))
                {
                    if (addOnly)
                        PlaybackManager.Instance.Playlist.AddRange(file, DefaultLoadErrorCallback);
                    else if (InstanceManager.Instance.PlaylistEditorInstance.ClosePlaylistQuery())
                        InstanceManager.Instance.PlaylistEditorInstance.InternalOpenPlaylist(file);
                    else
                        return false;
                }
                else if (PlaybackManager.Instance.HasSupportingPlayer(ext))
                {
                    var m = MusicInfo.Create(file, DefaultLoadErrorCallback);
                    if (m == null) return false;
                    PlaybackManager.Instance.Playlist.Add(m);
                    if (!addOnly) PlaybackManager.Instance.NowPlayingId = PlaybackManager.Instance.Playlist.Count - 1;
                }
                else
                {
                    MessageBox.Show("Unsupported file type: " + ext, "Non-critical error, everything is ok!",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }
            // by now we have determined that the stuff has been successfully added
            if (PlaybackManager.Instance.CurrentState != PlaybackState.Playing && !addOnly)
                PlaybackManager.Instance.PlayPauseResume();
            return true;
        }
    }
}
