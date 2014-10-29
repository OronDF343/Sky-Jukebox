using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SkyJukebox.CoreApi.Contracts;
using SkyJukebox.CoreApi.Playback;
using SkyJukebox.CoreApi.Utils;
using SkyJukebox.CoreApi.Xml;
using SkyJukebox.Icons;
using SkyJukebox.Playback;

namespace SkyJukebox
{
    public partial class PlaylistEditor : Form
    {
        private Settings _settings;
        private readonly ManagedListViewHelper<IMusicInfo> _playlistViewHelper;

        public PlaylistEditor()
        {
            _settings = Settings.Instance;
            InitializeComponent();
            addFilesToolStripButton.Image = IconManager.Instance.GetIcon("add16file").GetImage();
            addFolderToolStripButton.Image = IconManager.Instance.GetIcon("add16folder").GetImage();
            removeSelectedToolStripButton.Image = IconManager.Instance.GetIcon("remove16file").GetImage();
            removeAllToolStripButton.Image = IconManager.Instance.GetIcon("remove16all").GetImage();
            moveToTopToolStripButton.Image = IconManager.Instance.GetIcon("move16top").GetImage();
            moveUpToolStripButton.Image = IconManager.Instance.GetIcon("move16up").GetImage();
            moveDownToolStripButton.Image = IconManager.Instance.GetIcon("move16down").GetImage();
            moveToBottomToolStripButton.Image = IconManager.Instance.GetIcon("move16bottom").GetImage();
            openPlaylistToolStripButton.Image = IconManager.Instance.GetIcon("playlist16").GetImage();
            savePlaylistToolStripButton.Image = IconManager.Instance.GetIcon("save16").GetImage();
            savePlaylistAsToolStripButton.Image = IconManager.Instance.GetIcon("save16as").GetImage();
            _playlistViewHelper = new ManagedListViewHelper<IMusicInfo>(ref playlistManagedListView, new List<Column<IMusicInfo>> { new Column<IMusicInfo>("Name", m => m.FileName), new Column<IMusicInfo>("Type", m => m.Extension) }, PlaybackManager.Instance.Playlist);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mouse over a control, the tooltip explains it all!", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaybackManager.Instance.PlayPauseResume();
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaybackManager.Instance.Previous();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaybackManager.Instance.Next();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaybackManager.Instance.Stop();
        }

        private void shuffleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaybackManager.Instance.Shuffle = shuffleToolStripMenuItem.Checked;
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = true;
            currentSongToolStripMenuItem.Checked = false;
            entirePlaylistToolStripMenuItem.Checked = false;
            PlaybackManager.Instance.LoopType = LoopTypes.None;
        }

        private void currentSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            currentSongToolStripMenuItem.Checked = true;
            entirePlaylistToolStripMenuItem.Checked = false;
            PlaybackManager.Instance.LoopType = LoopTypes.Single;
        }

        private void entirePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            currentSongToolStripMenuItem.Checked = false;
            entirePlaylistToolStripMenuItem.Checked = true;
            PlaybackManager.Instance.LoopType = LoopTypes.All;
        }

        private void hidePlaylistEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void hideMiniPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.Hide();
        }

        private void hideAllWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            InstanceManager.MiniPlayerInstance.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.Close();
        }

        private void showMiniPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.Show();
        }
        private void RefreshPlaylist()
        {
            PlaybackManager.Instance.Playlist.Clear();
            PlaybackManager.Instance.Playlist.AddRange(_playlistViewHelper.Items);
            if (PlaybackManager.Instance.Playlist.ShuffleIndex)
                PlaybackManager.Instance.Playlist.Reshuffle();
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
                                         select new MusicInfo(f));
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
                    _playlistViewHelper.AddRange(from f in StringUtils.GetFiles(fbd.SelectedPath)
                                                 let m = new MusicInfo(f)
                                                 where PlaybackManager.Instance.HasSupportingPlayer(m.Extension)
                                                 select m);
                    RefreshPlaylist();
                    break;
                case DialogResult.No:
                    _playlistViewHelper.AddRange(from f in new DirectoryInfo(fbd.SelectedPath).GetFiles()
                                                 let m = new MusicInfo(f.FullName)
                                                 where PlaybackManager.Instance.HasSupportingPlayer(m.Extension)
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

        public bool ClosePlaylistQuery()
        {
            if (_playlistViewHelper.Items.Count <= 0) return true;
            var dr = MessageBox.Show("Save current playlist?", "Playlist", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (dr)
            {
                case DialogResult.Yes:
                    {
                        var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
                        if (sfd.ShowDialog() == DialogResult.OK)
                            StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, sfd.FileName, true);
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
            _playlistViewHelper.RemoveAll();
            RefreshPlaylist();
        }

        private void openPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ClosePlaylistQuery())
                return;
            _playlistViewHelper.RemoveAll();
            RefreshPlaylist();

            var ofdiag = new OpenFileDialog { Filter = "M3U/M3U8 Playlist (*.m3u*)|*.m3u*", Multiselect = false };
            if (ofdiag.ShowDialog() != DialogResult.OK) return;
            PlaybackManager.Instance.Playlist = new Playlist(ofdiag.FileName);
            _playlistViewHelper.AddRange(PlaybackManager.Instance.Playlist);
        }

        private void savePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
            if (sfd.ShowDialog() == DialogResult.OK)
                StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, sfd.FileName, true);
        }

        private void savePlaylistAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "M3U8 Playlist (*.m3u8)|*.m3u8" };
            if (sfd.ShowDialog() == DialogResult.OK)
                StringUtils.SavePlaylist(PlaybackManager.Instance.Playlist, sfd.FileName, true);
        }

        private void PlaylistEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            InstanceManager.PlaylistEditorInstance = null;
        }

        private void codecInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var players = from i in PlaybackManager.Instance.GetAudioPlayerInfo()
                          where i.Key != "SkyJukebox.Playback.NAudioPlayer"
                          select "* " + i.Key + ": " + string.Join(", ", i.Value);
            var codecs = from i in NAudioPlayer.GetCodecInfo()
                         select "* " + i.Key + ": " + string.Join(", ", i.Value);
            MessageBox.Show("Installed players:\n" + string.Join("\n", players) +
                            "* SkyJukebox.Playback.NAudioPlayer - Installed codecs:\n\t" + 
                            string.Join("\n\t", codecs));
        }

        private void aboutSkyJukeboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.9.0 \"Modular\" Alpha2.0", "About Sky Jukebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
