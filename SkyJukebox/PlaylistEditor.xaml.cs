#region Using statements
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using SkyJukebox.Api.Playback;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Playlist;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Collections;
using SkyJukebox.Lib.Wpf;
using SkyJukebox.Lib.Xml;
using SkyJukebox.Utils;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
#endregion

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for PlaylistEditor.xaml
    /// </summary>
    public partial class PlaylistEditor : INotifyPropertyChanged, IDisposable
    {
        #region File Filters
        private static string _audioFileFilter;
        public static string AudioFileFilter 
        { 
            get
            {
                if (_audioFileFilter != null) return _audioFileFilter;
                var exts = "*." + string.Join(";*.", from p in PlaybackManager.Instance.GetAudioPlayerInfo()
                                                     from e in p.Value
                                                     select e);
                return _audioFileFilter = string.Format("All supported formats ({0})|{1}", exts.Replace(";", ", "), exts);
            } 
        }

        private static string _playlistOpenFileFilter;
        public static string PlaylistOpenFileFilter
        {
            get
            {
                if (_playlistOpenFileFilter != null) return _playlistOpenFileFilter;
                var exts = "*." + string.Join(";*.", from p in PlaylistDataManager.Instance.GetAllReaders()
                                                     from e in p.FormatExtensions
                                                     select e);
                return _playlistOpenFileFilter = string.Format("All supported formats ({0})|{1}", exts.Replace(";", ", "), exts);
            }
        }

        private static string _playlistSaveFileFilter;
        public static string PlaylistSaveFileFilter
        {
            get
            {
                if (_playlistSaveFileFilter != null) return _playlistSaveFileFilter;
                return _playlistSaveFileFilter = string.Join("|", from w in PlaylistDataManager.Instance.GetAllWriters()
                                                                  let exts = "*." + string.Join(";*.", w.FormatExtensions)
                                                                  select string.Format("{0} ({1})|{2}", w.FormatName, exts.Replace(";", ", "), exts));
            }
        }
        #endregion

        public PlaylistEditor()
        {
            InitializeComponent();
            PlaybackManagerInstance.Playlist.CollectionChanged += Playlist_CollectionChanged;
            PlaybackManagerInstance.PropertyChanged += Instance_PropertyChanged;
            IconManagerInstance.CollectionChanged += (sender, args) => OnPropertyChanged("IconManagerInstance");
            ShowMiniPlayerMenuItem.IsChecked = InstanceManager.Instance.MiniPlayerInstance.IsVisible;
            CurrentPlaylist = null;
            TreeBrowser.FileExtensionFilter = PlaybackManagerInstance.SupportedFileTypes;
            foreach (var e in PlaylistDataManager.Instance.GetAllReaders().SelectMany(r => r.FormatExtensions))
                TreeBrowser.FileExtensionFilter.Add(e);
        }

        public IPlaylist Playlist { get { return PlaybackManagerInstance.Playlist; } }

        private void TrySavePlaylist(string path)
        {
            if (!FileUtils.SavePlaylist(path))
                MessageBox.Show("Error saving the playlist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Saving logic management
        private bool _dirty;
        private bool Dirty 
        { 
            get { return _dirty; }
            set { _dirty = value; Title = _dirty ? "Playlist Editor * - Sky Jukebox" : "Playlist Editor - Sky Jukebox"; } 
        }
        void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dirty = true;
        }

        public string CurrentPlaylist { get; set; }

        public bool ClosePlaylistQuery()
        {
            if (PlaybackManagerInstance.Playlist.Count <= 0 || !Dirty) return true;
            var dr = MessageBox.Show("Save changes to current playlist?", "Playlist", 
                                     MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                     MessageBoxResult.Cancel);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    {
                        if (CurrentPlaylist != null)
                            TrySavePlaylist(CurrentPlaylist);
                        else if (Sfd.ShowDialog() == true)
                            TrySavePlaylist(Sfd.FileName);
                        else
                            return false;
                    }
                    break;
                case MessageBoxResult.Cancel:
                    return false;
            }

            return true;
        }
        #endregion

        #region File menu

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!ClosePlaylistQuery()) return;
            PlaybackManagerInstance.Playlist.Clear();
            Dirty = true;
            CurrentPlaylist = null;
        }

        private OpenFileDialog _ofdPlaylist;
        internal void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!ClosePlaylistQuery())
                return;
            PlaybackManagerInstance.Playlist.Clear();
            if (_ofdPlaylist == null)
                _ofdPlaylist = new OpenFileDialog { Filter = PlaylistOpenFileFilter, 
                                                    Multiselect = false };
            if (_ofdPlaylist.ShowDialog() != true) return;
            InternalOpenPlaylist(_ofdPlaylist.FileName);
        }

        /// <summary>
        /// Do not use unless you know what you are doing!
        /// </summary>
        /// <param name="path">Path to the playlist.</param>
        internal void InternalOpenPlaylist(string path)
        {
            PlaybackManagerInstance.Playlist.AddRange(CurrentPlaylist = path, FileSystemUtils.DefaultLoadErrorCallback);
            Dirty = !PlaylistDataManager.Instance.HasWriter(CurrentPlaylist.GetExt());
        }

        private SaveFileDialog _sfd;
        private SaveFileDialog Sfd 
        {
            get { return _sfd ?? (_sfd = new SaveFileDialog { Filter = PlaylistSaveFileFilter }); } 
        }
        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!Dirty)
                return;
            if (CurrentPlaylist != null)
                TrySavePlaylist(CurrentPlaylist);
            else if (Sfd.ShowDialog() == true)
                TrySavePlaylist(CurrentPlaylist = Sfd.FileName);
            else
                return;

            Dirty = false;
        }

        private void SavePlaylistAs_Click(object sender, RoutedEventArgs e)
        {
            if (Sfd.ShowDialog() == true)
                TrySavePlaylist(CurrentPlaylist = Sfd.FileName);
            else
                return;

            Dirty = false;
        }

        private void ShowMiniPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (ShowMiniPlayerMenuItem.IsChecked)
                InstanceManager.Instance.MiniPlayerInstance.Show();
            else
                InstanceManager.Instance.MiniPlayerInstance.Hide();
        }

        private void HideThisWindow_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void HideAllWindows_Click(object sender, RoutedEventArgs e)
        {
            ShowMiniPlayerMenuItem.IsChecked = false;
            InstanceManager.Instance.MiniPlayerInstance.Hide();
            Hide();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            InstanceManager.Instance.MiniPlayerInstance.Close();
        }

        #endregion

        #region Edit menu

        private OpenFileDialog _ofdMedia;
        internal void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_ofdMedia == null)
                _ofdMedia = new OpenFileDialog { Multiselect = true, Filter = AudioFileFilter };
            if (_ofdMedia.ShowDialog() != true) return;
            PlaybackManagerInstance.Playlist.AddRange(from f in _ofdMedia.FileNames
                                                      let m = MusicInfo.Create(f, FileSystemUtils.DefaultLoadErrorCallback)
                                                      where m != null
                                                      select m);
        }

        private FolderBrowserDialog _fbd;
        internal void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_fbd == null) _fbd = new FolderBrowserDialog();
            if (_fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            FileSystemUtils.AddFolderQuery(_fbd.SelectedPath);
        }

        internal void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_ofdPlaylist == null)
                _ofdPlaylist = new OpenFileDialog
                {
                    Filter = "M3U/M3U8 Playlist (*.m3u*)|*.m3u*",
                    Multiselect = false
                };
            if (_ofdPlaylist.ShowDialog() != true) return;
            PlaybackManagerInstance.Playlist.AddRange(_ofdPlaylist.FileName, FileSystemUtils.DefaultLoadErrorCallback);
        }

        private void PlaylistView_OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            var gridView = PlaylistView.View as GridView;
            if (gridView == null) return;
            foreach (var column in gridView.Columns)
            {
                if (double.IsNaN(column.Width))
                    column.Width = column.ActualWidth;
                column.Width = double.NaN;
            }
        }

        private void MoveToTop_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManagerInstance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = 0; i < selected.Count; ++i)
                PlaybackManagerInstance.Playlist.Move(selected[i], i);
        }

        private void MoveToBottom_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManagerInstance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManagerInstance.Playlist.Move(selected[i], PlaybackManagerInstance.Playlist.Count - selected.Count + i);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManagerInstance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < selected.Count; ++i)
                PlaybackManagerInstance.Playlist.Move(selected[i], selected[i] - 1);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManagerInstance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManagerInstance.Playlist.Move(selected[i], selected[i] + 1);
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = (from m in PlaylistView.SelectedItems.Cast<IMusicInfo>()
                            select PlaybackManagerInstance.Playlist.IndexOf(m)).ToList();
            selected.Sort();
            for (int i = selected.Count - 1; i > -1; --i)
                PlaybackManagerInstance.Playlist.RemoveAt(selected[i]);
        }

        private void RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the playlist?", "Remove All", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                PlaybackManagerInstance.Playlist.Clear();
        }

        #region Sorting

        private static readonly Dictionary<string, Func<IMusicInfo, string>> SortBy = new Dictionary
            <string, Func<IMusicInfo, string>>
        {
            // TODO: replace with the localized strings or get the corresponding en-US/default string
            { "By File Name", info => info.FileName },
            { "By Title", info => info.Tag.Title },
            { "By Performers", info => info.Tag.FirstPerformer }, // no point in using joined stuff here
            { "By Album Artists", info => info.Tag.FirstAlbumArtist },
            { "By Album", info => info.Tag.Album },
            { "By Track Number", info => info.Tag.Track.ToString(CultureInfo.InvariantCulture) },
            { "By Genre", info => info.Tag.FirstGenre },
            { "By Year", info => info.Tag.Year.ToString(CultureInfo.InvariantCulture) },
            { "By Duration", info => info.Duration.ToString() },
            { "By Codec", info => info.Extension },
            { "By Bitrate", info => info.Bitrate.ToString(CultureInfo.InvariantCulture) },
        };
        private void SortBy_Click(object sender, RoutedEventArgs e)
        {
            var header = "";
            var item = sender as MenuItem;
            if (item != null)
            {
                header = item.Header.ToString().Replace("_", "");
            }
            var column = sender as GridViewColumnHeader;
            if (column != null)
            {
                header = "By " + column.Content.ToString().Replace("_", "");
            }
            if (string.IsNullOrEmpty(header)) return;
            PlaybackManagerInstance.Playlist.Sort((x, y) =>
                                                  (ReverseOrderMenuItem.IsChecked ? -1 : 1) *
                                                  String.Compare(SortBy[header](x),
                                                                 SortBy[header](y),
                                                                 StringComparison.Ordinal));
        }

        #endregion
        #endregion

        #region View menu

        public ObservableDictionary<string, Property> ColumnVisibilitySettings
        {
            get { return (ObservableDictionary<string, Property>)SettingsManager.Instance["PlaylistEditorColumnsVisibility"].Value; }
        }
        #endregion

        #region Playback menu
        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LoopType":
                    switch (PlaybackManagerInstance.LoopType)
                    {
                        case LoopTypes.None:
                            OnPropertyChanged("IsLoopTypeNone");
                            break;
                        case LoopTypes.Single:
                            OnPropertyChanged("IsLoopTypeSingle");
                            break;
                        default:
                            OnPropertyChanged("IsLoopTypeAll");
                            break;
                    }
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsLoopTypeNone
        {
            get { return PlaybackManagerInstance.LoopType == LoopTypes.None; }
            set { if (value) PlaybackManagerInstance.LoopType = LoopTypes.None; }
        }

        public bool IsLoopTypeSingle
        {
            get { return PlaybackManagerInstance.LoopType == LoopTypes.Single; }
            set { if (value) PlaybackManagerInstance.LoopType = LoopTypes.Single; }
        }

        public bool IsLoopTypeAll
        {
            get { return PlaybackManagerInstance.LoopType == LoopTypes.All; }
            set { if (value) PlaybackManagerInstance.LoopType = LoopTypes.All; }
        }

        private void PlayPauseResume_Click(object sender, RoutedEventArgs e)
        {
            PlaybackManagerInstance.PlayPauseResume();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            PlaybackManagerInstance.Stop();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            PlaybackManagerInstance.Previous();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            PlaybackManagerInstance.Next();
        }
        #endregion

        #region Tools menu

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            InstanceManager.Instance.SettingsWindowInstance.Show();
        }
        #endregion

        #region Plugins menu
        #endregion

        #region Help menu

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(StringUtils.GetSkyJukeboxAboutString(), "About", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        #endregion

        #region Closing logic
        private bool _close;
        private void Window_Closing(object sender, CancelEventArgs e)
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
        #endregion

        private void PlaylistView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dep = (e.OriginalSource as DependencyObject).VisualUpwardSearch<System.Windows.Controls.ListViewItem>();
            if (dep == null) return;
            var item = (IMusicInfo)PlaylistView.ItemContainerGenerator.ItemFromContainer(dep);
            var index = Playlist.ShuffledIndexOf(item);

            if (index < 0 || index >= Playlist.Count) return;
            PlaybackManagerInstance.NowPlayingId = index;
            if (PlaybackManagerInstance.CurrentState != PlaybackState.Playing) PlaybackManagerInstance.PlayPauseResume();
        }

        public PlaybackManager PlaybackManagerInstance { get { return PlaybackManager.Instance; } }

        public IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }

        #region TreeBrowser

        private async void AddFromTreeBrowser_OnClick(object sender, RoutedEventArgs e)
        {
            SpinningGear.Visibility = Visibility.Visible;
            var s = from r in TreeBrowser.RootList
                    from n in r.GetChecked()
                    select n;
            foreach (var infoEx in s)
            {
                if (infoEx.IsFolder)
                    await FileUtils.AddFolder(infoEx as DirectoryInfoEx, true, FileSystemUtils.DefaultLoadErrorCallback);
                else if (PlaylistDataManager.Instance.HasReader(infoEx.Name.GetExt()))
                    Playlist.AddRange(infoEx.FullName, FileSystemUtils.DefaultLoadErrorCallback);
                else if (PlaybackManagerInstance.HasSupportingPlayer(infoEx.Name.GetExt()))
                    Playlist.Add(MusicInfo.Create(infoEx as FileInfoEx, FileSystemUtils.DefaultLoadErrorCallback));
            }
            SpinningGear.Visibility = Visibility.Hidden;
        }

        private void DeselectTreeBrowser_OnClick(object sender, RoutedEventArgs e)
        {
            TreeBrowser.RootList.ForEach(f => f.ForceDeselectAll());
        }

        private void RefreshTreeBrowser_OnClick(object sender, RoutedEventArgs e)
        {
            TreeBrowser.Refresh();
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _fbd.Dispose();
        }

        #region Drag and Drop support

        private void PlaylistView_OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, false) || sender == e.Source)
                e.Effects = DragDropEffects.None;
        }

        private async void PlaylistView_OnDrop(object sender, DragEventArgs e)
        {
            SpinningGear.Visibility = Visibility.Visible;
            foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (DirectoryEx.Exists(s))
                    await FileUtils.AddFolder(FileSystemInfoEx.FromString(s) as DirectoryInfoEx, true, FileSystemUtils.DefaultLoadErrorCallback);
                else if (PlaylistDataManager.Instance.HasReader(s.GetExt()))
                    Playlist.AddRange(s, FileSystemUtils.DefaultLoadErrorCallback);
                else if (PlaybackManagerInstance.HasSupportingPlayer(s.GetExt()))
                    Playlist.Add(MusicInfo.Create(FileSystemInfoEx.FromString(s) as FileInfoEx, FileSystemUtils.DefaultLoadErrorCallback));
            }
            SpinningGear.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Toolbar extras

        private void SortDirection_Click(object sender, RoutedEventArgs e)
        {
            ReverseOrderMenuItem.IsChecked = !ReverseOrderMenuItem.IsChecked;
        }

        private void Shuffle_OnClick(object sender, RoutedEventArgs e)
        {
            PlaybackManagerInstance.Shuffle = !PlaybackManagerInstance.Shuffle;
        }

        private void Loop_OnClick(object sender, RoutedEventArgs e)
        {
            switch (PlaybackManagerInstance.LoopType)
            {
                case LoopTypes.None:
                    PlaybackManagerInstance.LoopType = LoopTypes.Single;
                    break;
                case LoopTypes.Single:
                    PlaybackManagerInstance.LoopType = LoopTypes.All;
                    break;
                default:
                    PlaybackManagerInstance.LoopType = LoopTypes.None;
                    break;
            }
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if (PlaybackManagerInstance.Volume == 0)
                PlaybackManagerInstance.Volume = InstanceManager.Instance.TempVolume > 0 ? InstanceManager.Instance.TempVolume : 1;
            else
            {
                InstanceManager.Instance.TempVolume = PlaybackManagerInstance.Volume;
                PlaybackManagerInstance.Volume = 0;
            }
        }
        #endregion
    }

    public class IndexCompareConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IPlaylist)values[1]).IndexOf((IMusicInfo)values[0]) == (int)values[2];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrdinalIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (((IPlaylist)values[1]).IndexOf((IMusicInfo)values[0]) + 1).ToString(CultureInfo.InvariantCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
