﻿#region Using statements
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
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Collections;
using SkyJukebox.Lib.Wpf;
using SkyJukebox.Lib.Xml;
using SkyJukebox.Utils;
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
    public partial class PlaylistEditor : INotifyPropertyChanged
    {
        private static string _fileFilter;
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
            //PlaylistView.ItemsSource = PlaybackManagerInstance.Playlist;
            PlaybackManagerInstance.Playlist.CollectionChanged += Playlist_CollectionChanged;
            PlaybackManagerInstance.PropertyChanged += Instance_PropertyChanged;
            ShowMiniPlayerMenuItem.IsChecked = InstanceManager.MiniPlayerInstance.IsVisible;
            CurrentPlaylist = null;
            TreeBrowser.FileExtensionFilter = PlaybackManagerInstance.SupportedFileTypes;
            TreeBrowser.FileExtensionFilter.Add("m3u");
            TreeBrowser.FileExtensionFilter.Add("m3u8");
        }

        public IPlaylist Playlist { get { return PlaybackManagerInstance.Playlist; } }

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
                            StringUtils.SavePlaylist(PlaybackManagerInstance.Playlist, CurrentPlaylist, true);
                        else if (Sfd.ShowDialog() == true)
                            StringUtils.SavePlaylist(PlaybackManagerInstance.Playlist, Sfd.FileName, true);
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
                _ofdPlaylist = new OpenFileDialog { Filter = "M3U/M3U8 Playlist (*.m3u*)|*.m3u*", 
                                                    Multiselect = false };
            if (_ofdPlaylist.ShowDialog() != true) return;
            PlaybackManagerInstance.Playlist.AddRange(CurrentPlaylist = _ofdPlaylist.FileName);
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
                StringUtils.SavePlaylist(PlaybackManagerInstance.Playlist, CurrentPlaylist, true);
            else if (Sfd.ShowDialog() == true)
                StringUtils.SavePlaylist(PlaybackManagerInstance.Playlist, CurrentPlaylist = Sfd.FileName, true);
            else
                return;

            Dirty = false;
        }

        private void SavePlaylistAs_Click(object sender, RoutedEventArgs e)
        {
            if (Sfd.ShowDialog() == true)
                StringUtils.SavePlaylist(PlaybackManagerInstance.Playlist, CurrentPlaylist = Sfd.FileName, true);
            else
                return;

            Dirty = false;
        }

        private void ShowMiniPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (ShowMiniPlayerMenuItem.IsChecked)
                InstanceManager.MiniPlayerInstance.Show();
            else
                InstanceManager.MiniPlayerInstance.Hide();
        }

        private void HideThisWindow_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void HideAllWindows_Click(object sender, RoutedEventArgs e)
        {
            ShowMiniPlayerMenuItem.IsChecked = false;
            InstanceManager.MiniPlayerInstance.Hide();
            Hide();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            InstanceManager.MiniPlayerInstance.Close();
        }

        #endregion

        #region Edit menu

        private OpenFileDialog _ofdMedia;
        internal void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_ofdMedia == null)
                _ofdMedia = new OpenFileDialog { Multiselect = true, Filter = FileFilter };
            if (_ofdMedia.ShowDialog() != true) return;
            PlaybackManagerInstance.Playlist.AddRange(from f in _ofdMedia.FileNames
                                                       select new MusicInfo(f));
        }

        private FolderBrowserDialog _fbd;
        internal void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_fbd == null) _fbd = new FolderBrowserDialog();
            if (_fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            DirUtils.AddFolderQuery(_fbd.SelectedPath);
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
            PlaybackManagerInstance.Playlist.AddRange(_ofdPlaylist.FileName);
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
            var header = ((MenuItem)sender).Header.ToString().Replace("_", "");
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
                case "Shuffle":
                    OnPropertyChanged("IsShuffleOn");
                    break;
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
                case "CurrentState":
                    OnPropertyChanged("PlayMenuItemHeader");
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsShuffleOn
        {
            get { return PlaybackManagerInstance.Shuffle; }
            set { PlaybackManagerInstance.Shuffle = value; }
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

        public string PlayMenuItemHeader
        {
            get
            {
                switch (PlaybackManagerInstance.CurrentState)
                {
                    case PlaybackManager.PlaybackStates.Playing:
                        return "_Pause";
                    case PlaybackManager.PlaybackStates.Paused:
                        return "_Resume";
                    default:
                        return "_Play";
                }
            }
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
            InstanceManager.SettingsWindowInstance.Show();
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
            if (PlaybackManagerInstance.CurrentState != PlaybackManager.PlaybackStates.Playing) PlaybackManagerInstance.PlayPauseResume();
        }

        public PlaybackManager PlaybackManagerInstance { get { return PlaybackManager.Instance; } }

        private async void AddFromTreeBrowser_OnClick(object sender, RoutedEventArgs e)
        {
            SpinningGear.Visibility = Visibility.Visible;
            var s = from r in TreeBrowser.RootList
                    from n in r.GetChecked()
                    select n;
            foreach (var infoEx in s)
            {
                if (infoEx.IsFolder)
                    await FileUtils.AddFolder(infoEx as DirectoryInfoEx, true);
                else if (infoEx.Name.GetExt().StartsWith("m3u"))
                    Playlist.AddRange(infoEx.FullName);
                else if (PlaybackManagerInstance.HasSupportingPlayer(infoEx.Name.GetExt()))
                    Playlist.Add(infoEx.FullName);
            }
            SpinningGear.Visibility = Visibility.Hidden;
        }
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
