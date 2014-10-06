using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SkyJukebox.Data;
using SkyJukebox.Playback;

namespace SkyJukebox
{
    public partial class PlaylistEditor : Form
    {
        private Settings _settings;
        private readonly ManagedListViewHelper<Music> _playlistViewHelper;

        public PlaylistEditor()
        {
            _settings = Instance.Settings;
            InitializeComponent();
            addFilesToolStripButton.Image = Instance.IconImageDictionary["add16file"];
            addFolderToolStripButton.Image = Instance.IconImageDictionary["add16folder"];
            removeSelectedToolStripButton.Image = Instance.IconImageDictionary["remove16file"];
            removeAllToolStripButton.Image = Instance.IconImageDictionary["remove16all"];
            moveToTopToolStripButton.Image = Instance.IconImageDictionary["move16top"];
            moveUpToolStripButton.Image = Instance.IconImageDictionary["move16up"];
            moveDownToolStripButton.Image = Instance.IconImageDictionary["move16down"];
            moveToBottomToolStripButton.Image = Instance.IconImageDictionary["move16bottom"];
            openPlaylistToolStripButton.Image = Instance.IconImageDictionary["playlist16"];
            savePlaylistToolStripButton.Image = Instance.IconImageDictionary["save16"];
            savePlaylistAsToolStripButton.Image = Instance.IconImageDictionary["save16as"];
            _playlistViewHelper = new ManagedListViewHelper<Music>(ref playlistManagedListView, new List<Column<Music>> { new Column<Music>("Name", m => m.FileName), new Column<Music>("Type", m => m.Extension) }, Instance.BgPlayer.Playlist);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mouse over a control, the tooltip explains it all!", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (Instance.BgPlayer.Status)
            {
                case PlaybackStatus.Stopped:
                    Instance.BgPlayer.Play();
                    break;
                case PlaybackStatus.Paused:
                    Instance.BgPlayer.Resume();
                    break;
                default:
                    Instance.BgPlayer.Pause();
                    break;
            }
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.BgPlayer.Previous();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.BgPlayer.Next();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.BgPlayer.Stop();
        }

        private void shuffleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.BgPlayer.Shuffle = shuffleToolStripMenuItem.Checked;
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = true;
            currentSongToolStripMenuItem.Checked = false;
            entirePlaylistToolStripMenuItem.Checked = false;
            Instance.BgPlayer.LoopType = LoopType.None;
        }

        private void currentSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            currentSongToolStripMenuItem.Checked = true;
            entirePlaylistToolStripMenuItem.Checked = false;
            Instance.BgPlayer.LoopType = LoopType.Single;
        }

        private void entirePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            currentSongToolStripMenuItem.Checked = false;
            entirePlaylistToolStripMenuItem.Checked = true;
            Instance.BgPlayer.LoopType = LoopType.All;
        }

        private void hidePlaylistEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void hideMiniPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.Hide();
        }

        private void hideAllWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            Instance.MiniPlayerInstance.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.Close();
        }

        private void showMiniPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.Show();
        }
        private void RefreshPlaylist()
        {
            Instance.BgPlayer.Playlist.Clear();
            Instance.BgPlayer.Playlist.AddRange(_playlistViewHelper.Items);
            if (Instance.BgPlayer.Playlist.ShuffleIndex)
                Instance.BgPlayer.Playlist.Reshuffle();
        }

        private void addFilesToolStripButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter =
                    "All audio files (*.wav, *.mp3, *.wma, *.flac, *.ogg, *.m4a, *.aiff)|*.wav;*.mp3;*.wma;*.flac;*.ogg;*.m4a;*.aiff|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            _playlistViewHelper.AddRange(from f in ofd.FileNames
                                         select new Music(f));
            RefreshPlaylist();
        }

        private void addFolderToolStripButton_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK) return;
            var dr = DialogResult.No;
            if (new DirectoryInfo(fbd.SelectedPath).GetDirectories().Length > 0)
                dr = MessageBox.Show("Import subfolders?", "Add Folder", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (dr)
            {
                case DialogResult.Yes:
                    _playlistViewHelper.AddRange(from f in Util.GetFiles(fbd.SelectedPath)
                                                 let m = new Music(f)
                                                 where BackgroundPlayer.HasCodec(m.Extension)
                                                 select m);
                    RefreshPlaylist();
                    break;
                case DialogResult.No:
                    _playlistViewHelper.AddRange(from f in new DirectoryInfo(fbd.SelectedPath).GetFiles()
                                                 let m = new Music(f.FullName)
                                                 where BackgroundPlayer.HasCodec(m.Extension)
                                                 select m);
                    RefreshPlaylist();
                    break;
            }
        }

        private void removeSelectedToolStripButton_Click(object sender, EventArgs e)
        {
            _playlistViewHelper.RemoveSelected();
            RefreshPlaylist();
        }

        private void removeAllToolStripButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove all the items?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) ==
                DialogResult.Yes)
            {
                _playlistViewHelper.RemoveAll();
                RefreshPlaylist();
            }
        }

        private void moveToTopToolStripButton_Click(object sender, EventArgs e)
        {
            _playlistViewHelper.MoveToTop();
            RefreshPlaylist();
        }

        private void moveUpToolStripButton_Click(object sender, EventArgs e)
        {
            _playlistViewHelper.MoveUp();
            RefreshPlaylist();
        }

        private void moveDownToolStripButton_Click(object sender, EventArgs e)
        {
            _playlistViewHelper.MoveDown();
            RefreshPlaylist();
        }

        private void moveToBottomToolStripButton_Click(object sender, EventArgs e)
        {
            _playlistViewHelper.MoveToBottom();
            RefreshPlaylist();
        }

        private bool ClosePlaylistQuery()
        {
            if (_playlistViewHelper.Items.Count <= 0) return true;
            var dr = MessageBox.Show("Save current playlist?", "Playlist", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (dr)
            {
                case DialogResult.Yes:
                    {
                        var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
                        if (sfd.ShowDialog() == DialogResult.OK)
                            Util.SavePlaylist(Instance.BgPlayer.Playlist, sfd.FileName, true);
                        else
                            return false;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
            }

            return true;
        }

        private void newPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ClosePlaylistQuery())
                return;
        }

        private void openPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ClosePlaylistQuery())
                return;
            _playlistViewHelper.RemoveAll();
            RefreshPlaylist();

            var ofdiag = new OpenFileDialog { Filter = "M3U/M3U8 Playlist (*.m3u*)|*.m3u*", Multiselect = false };
            if (ofdiag.ShowDialog() != DialogResult.OK) return;
            Instance.BgPlayer.Playlist = new Playlist(ofdiag.FileName);
            _playlistViewHelper.AddRange(Instance.BgPlayer.Playlist);
        }

        private void savePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
            if (sfd.ShowDialog() == DialogResult.OK)
                Util.SavePlaylist(Instance.BgPlayer.Playlist, sfd.FileName, true);
        }

        private void savePlaylistAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
            if (sfd.ShowDialog() == DialogResult.OK)
                Util.SavePlaylist(Instance.BgPlayer.Playlist, sfd.FileName, true);
        }

        private void PlaylistEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance.PlaylistEditorInstance = null;
        }

        private void codecInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var codecs = (from i in Instance.LoadedCodecs
                          select "* " + i.WaveStreamType.FullName + ": " + string.Join(", ", i.Extensions)).ToList();
            MessageBox.Show("Installed codecs:\n" + string.Join("\n", codecs));
        }

        private void aboutSkyJukeboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.8.0", "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
