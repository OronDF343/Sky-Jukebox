using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace SkyJukebox.Playback
{
    public sealed class BackgroundPlayer : IDisposable
    {
        // Plugin support
        private static readonly Dictionary<IEnumerable<string>, Type> Codecs = new Dictionary<IEnumerable<string>,Type>();
        public static void AddCodec(IEnumerable<string> exts, Type t)
        {
            Codecs.Add(exts, t);
        }
        public static bool HasCodec(string ext)
        {
            return Codecs.Keys.Count(es => es.Contains(ext.ToLowerInvariant())) > 0;
        }

        private IWavePlayer _myWaveOut;
        private WaveStream _myAudioFileReader;
        public BackgroundPlayer()
        {
            Playlist = new Playlist();
            AutoPlay = true;
            Status = PlaybackStatus.Stopped;
            LoopType = LoopType.None;
        }
        public BackgroundPlayer(Playlist p)
        {
            Playlist = p;
            AutoPlay = true;
            Status = PlaybackStatus.Stopped;
            LoopType = LoopType.None;
        }
        public Playlist Playlist { get; set; }
        public PlaybackStatus Status { get; private set; }
        private int _nowPlayingId;
        public int NowPlayingId
        {
            get { return _nowPlayingId; }
            private set
            {
                _nowPlayingId = value;
                if (value >= Playlist.Count || value < 0)
                    Status = PlaybackStatus.Stopped;
                else if (Status == PlaybackStatus.Playing)
                    Play();
                else
                {
                    Unload();
                    Load();
                }
                FirePlaybackEvent(Status);
            }
        }
        public Music NowPlaying { get { return Playlist[NowPlayingId]; } }
        private bool _shuffle;
        public bool Shuffle
        {
            get { return _shuffle; }
            set
            {
                _shuffle = value;
                Playlist.ShuffleIndex = value;
                if (Status == PlaybackStatus.Stopped && Playlist.Count != 0)
                    NowPlayingId = 0;
            }
        }
        public bool AutoPlay { get; set; }
        public LoopType LoopType { get; set; }
        private float _volume;
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                _myWaveOut.Volume = value;
            }
        }
        public TimeSpan Position
        {
            get
            {
                return _myAudioFileReader != null ? _myAudioFileReader.CurrentTime : new TimeSpan();
            }
            set
            {
                if (_myAudioFileReader != null)
                    _myAudioFileReader.CurrentTime = value;
            }
        }
        public TimeSpan Duration
        {
            get
            {
                return _myAudioFileReader != null ? _myAudioFileReader.TotalTime : new TimeSpan();
            }
        }

        public event EventHandler<PlaybackEventArgs> PlaybackEvent;

        public void Jump(int id)
        {
            if (id < Playlist.Count)
                NowPlayingId = id;
            else
                throw new IndexOutOfRangeException("Now playing ID > Playlist length");
        }

        public void Next()
        {
            if (Playlist.Count == 0) return;
            if (NowPlayingId < Playlist.Count - 1)
                ++NowPlayingId;
            else
            {
                if (LoopType == LoopType.None)
                    Status = PlaybackStatus.Stopped;
                NowPlayingId = 0;
            }
        }

        public void Previous()
        {
            if (Playlist.Count == 0) return;
            if (NowPlayingId >= Playlist.Count)
                NowPlayingId = Playlist.Count - 1;
            else if (NowPlayingId > 0)
                --NowPlayingId;
            else
            {
                if (LoopType == LoopType.None)
                    Status = PlaybackStatus.Stopped;
                NowPlayingId = Playlist.Count - 1;
            }
        }
        /// <summary>
        /// Plays the current song from the start. To resume, use Resume().
        /// </summary>
        public void Play()
        {
            if (NowPlayingId >= Playlist.Count) return;
            Unload();
            Load();
            _myWaveOut.Play();
            Status = PlaybackStatus.Playing;
            FirePlaybackEvent(PlaybackStatus.Playing);
        }

        public void Pause()
        {
            _myWaveOut.Pause();
            Status = PlaybackStatus.Paused;
            FirePlaybackEvent(PlaybackStatus.Paused);
        }

        public void Resume()
        {
            if (Status == PlaybackStatus.Paused)
            {
                _myWaveOut.Play();
                Status = PlaybackStatus.Playing;
                FirePlaybackEvent(PlaybackStatus.Resumed);
            }
            else // Dummy
                _myWaveOut.Play();
        }

        public void Stop()
        {
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            if (Status == PlaybackStatus.Stopped) return;
            Status = PlaybackStatus.Stopped;
            FirePlaybackEvent(PlaybackStatus.Stopped);
        }

        private void FirePlaybackEvent(PlaybackStatus status)
        {
            if (PlaybackEvent == null) return;
            if (NowPlayingId < Playlist.Count && NowPlayingId >= 0)
                PlaybackEvent(this, new PlaybackEventArgs(status, NowPlayingId, NowPlaying.FilePath));
            else
                PlaybackEvent(this, new PlaybackEventArgs(status, NowPlayingId, "[Missing]", false, Playlist.Count == 0 ? "Empty playlist" : "Removed from playlist"));
        }

        public void Load()
        {
            var di = DirectSoundOut.Devices.FirstOrDefault(d => d.Guid == Instance.Settings.PlaybackDevice);
            _myWaveOut = new DirectSoundOut((di ?? (new DirectSoundDeviceInfo { Guid = DirectSoundOut.DSDEVID_DefaultPlayback })).Guid);
            try
            {
                _myAudioFileReader = Activator.CreateInstance((from c in Codecs
                                                               where c.Key.Contains(NowPlaying.Extension)
                                                               select c.Value).First(), NowPlaying.FilePath) as WaveStream;
            }
            finally
            {
                if (_myAudioFileReader == null) throw new NullReferenceException("Failed to create WaveStream! Invalid or missing codec!");
            }
            if (NowPlaying.Extension == "flac") Status = PlaybackStatus.Stopped; // don't remember why this was needed, temporary code.
            _myWaveOut.PlaybackStopped += myWaveOut_PlaybackStopped;
            _myWaveOut.Init(_myAudioFileReader);
        }

        public void Unload()
        {
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            if (_myAudioFileReader != null)
            {
                _myAudioFileReader.Dispose();
                _myAudioFileReader = null;
            }
            if (_myWaveOut != null)
            {
                _myWaveOut.Dispose();
                _myWaveOut = null;
            }

            Status = PlaybackStatus.Stopped;
            if (PlaybackEvent != null)
                PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Stopped, NowPlayingId, NowPlaying.FilePath));
        }

        public void Dispose()
        {
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            if (_myAudioFileReader != null)
            {
                _myAudioFileReader.Dispose();
                _myAudioFileReader = null;
            }
            if (_myWaveOut != null)
            {
                _myWaveOut.Dispose();
                _myWaveOut = null;
            }
        }

        private void myWaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //Status = PlaybackStatus.Stopped;
            if (PlaybackEvent != null)
                PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Stopped, NowPlayingId, NowPlaying.FilePath));
            if (!AutoPlay)
                Status = PlaybackStatus.Stopped;
            if (LoopType == LoopType.Single)
                Resume();
            else if (e.Exception == null)
                Next();
            //else
            //MessageBox.Show(e.Exception.Message);
        }
    }

    public class PlaybackEventArgs : EventArgs
    {
        public bool IsError { get; set; }
        public string NewTrackName { get; set; }
        public int NewTrackId { get; set; }
        public PlaybackStatus NewStatus { get; set; }
        public string Message { get; set; }
        public PlaybackEventArgs(PlaybackStatus status, int newTrackId, string newTrackName, bool isError = false, string msg = "")
        {
            NewStatus = status;
            NewTrackId = newTrackId;
            NewTrackName = newTrackName;
            IsError = isError;
            Message = msg;
        }
    }

    public enum PlaybackStatus
    {
        Playing, Paused, Resumed, Stopped
    }

    public enum LoopType
    {
        None, Single, All
    }
}
