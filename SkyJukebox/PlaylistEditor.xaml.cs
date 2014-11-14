using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for PlaylistEditor.xaml
    /// </summary>
    public partial class PlaylistEditor
    {
        private static string _fileFilter = null;
        public static string FileFilter 
        { 
            get
            {
                if (_fileFilter != null) return _fileFilter;
                var exts = "*." + string.Join(";*.", from p in PlaybackManager.Instance.GetAudioPlayerInfo()
                                                     from e in p.Value
                                                     select e);
                return _fileFilter = string.Format("All supported formats ({0})|{1}", exts.Replace(";", ", "), exts);
            } 
        }
        public PlaylistEditor()
        {
            InitializeComponent();
            PlaylistView.ItemsSource = PlaybackManager.Instance.Playlist;
            PlaybackManager.Instance.Playlist.CollectionChanged += Playlist_CollectionChanged;
        }

        private bool _dirty;
        private bool Dirty 
        { 
            get { return _dirty; }
            set { _dirty = value; Title = _dirty ? "Playlist Editor * - Sky Jukebox" : "Playlist Editor - Sky Jukebox"; } 
        }
        void Playlist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dirty = true;
        }

        public string CurrentPlaylist { get; set; }

        public bool ClosePlaylistQuery()
        {
            if (PlaybackManager.Instance.Playlist.Count <= 0 || !Dirty) return true;
            var dr = MessageBox.Show("Save changes to current playlist?", "Playlist", 
                                     MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                     MessageBoxResult.Cancel);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    {
                        if (CurrentPlaylist != null)
                            StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, CurrentPlaylist, true);
                        else if (Sfd.ShowDialog() == true)
                            StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, Sfd.FileName, true);
                        else
                            return false;
                    }
                    break;
                case MessageBoxResult.Cancel:
                    return false;
            }

            return true;
        }

        #region File menu

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!InstanceManager.ClosePlaylistQuery()) return;
            PlaybackManager.Instance.Playlist.Clear();
            Dirty = true;

        }

        private OpenFileDialog _ofdPlaylist;
        private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!InstanceManager.ClosePlaylistQuery())
                return;
            PlaybackManager.Instance.Playlist.Clear();
            if (_ofdPlaylist == null)
                _ofdPlaylist = new OpenFileDialog { Filter = "M3U/M3U8 Playlist (*.m3u*)|*.m3u*", 
                                                    Multiselect = false };
            if (_ofdPlaylist.ShowDialog() != true) return;
            PlaybackManager.Instance.Playlist = new Playlist(CurrentPlaylist = _ofdPlaylist.FileName);
            Dirty = false;
        }

        private SaveFileDialog _sfd;
        private SaveFileDialog Sfd 
        {
            get { return _sfd ?? (_sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" }); } 
        }
        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!Dirty)
                return;
            if (CurrentPlaylist != null)
                StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, CurrentPlaylist, true);
            else if (Sfd.ShowDialog() == true)
                StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, CurrentPlaylist = Sfd.FileName, true);
            else
                return;

            Dirty = false;
        }

        private void SavePlaylistAs_Click(object sender, RoutedEventArgs e)
        {
            if (Sfd.ShowDialog() == true)
                StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, CurrentPlaylist = Sfd.FileName, true);
            else
                return;

            Dirty = false;
        }
        #endregion

        #region Edit menu

        private OpenFileDialog _ofdMedia;
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_ofdMedia == null)
                _ofdMedia = new OpenFileDialog { Multiselect = true, Filter = FileFilter };
            if (_ofdMedia.ShowDialog() != true) return;
            PlaybackManager.Instance.Playlist.AddRange(from f in _ofdMedia.FileNames
                                                       select new MusicInfo(f));
        }

        private FolderBrowserDialog _fbd;
        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_fbd == null) _fbd = new FolderBrowserDialog();
            if (_fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var dr = MessageBoxResult.No;
            if (new DirectoryInfo(_fbd.SelectedPath).GetDirectories().Length > 0)
                dr = MessageBox.Show("Import subfolders?", "Add Folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    PlaybackManager.Instance.Playlist.AddRange(from f in StringUtils.GetFiles(_fbd.SelectedPath)
                                                               let m = new MusicInfo(f)
                                                               where PlaybackManager.Instance.HasSupportingPlayer(m.Extension)
                                                               select m);
                    break;
                case MessageBoxResult.No:
                    PlaybackManager.Instance.Playlist.AddRange(from f in new DirectoryInfo(_fbd.SelectedPath).GetFiles()
                                                               let m = new MusicInfo(f.FullName)
                                                               where PlaybackManager.Instance.HasSupportingPlayer(m.Extension)
                                                               select m);
                    break;
            }
        }

        private void MoveToTop_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManager.Instance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = 0; i < selected.Count; ++i)
                PlaybackManager.Instance.Playlist.Move(selected[i], i);
        }

        private void MoveToBottom_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManager.Instance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManager.Instance.Playlist.Move(selected[i], PlaybackManager.Instance.Playlist.Count - selected.Count + i);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManager.Instance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < selected.Count; ++i)
                PlaybackManager.Instance.Playlist.Move(selected[i], selected[i] - 1);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManager.Instance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManager.Instance.Playlist.Move(selected[i], selected[i] + 1);
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManager.Instance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManager.Instance.Playlist.RemoveAt(selected[i]);
        }

        private void RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the playlist?", "Remove All", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                PlaybackManager.Instance.Playlist.Clear();
        }
        #endregion

        private bool _close;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_close) return;
            e.Cancel = true;
            Hide();
        }

        public void CloseFinal()
        {
            _close = true;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_fbd != null)
                _fbd.Dispose();
            _fbd = null;
        }
    }
}
