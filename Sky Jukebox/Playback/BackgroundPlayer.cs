﻿using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SkyJukebox
{
    public class BackgroundPlayer : IDisposable
    {
        // Plugin support
        private static Dictionary<IEnumerable<string>, Type> codecs = new Dictionary<IEnumerable<string>,Type>();
        public static void AddCodec(IEnumerable<string> exts, Type t)
        {
            codecs.Add(exts, t);
        }
        public static bool HasCodec(string ext)
        {
            return codecs.Keys.Count(es => es.Contains(ext.ToLowerInvariant())) > 0;
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
                if (Status == PlaybackStatus.Playing)
                    Play();
                else
                {
                    Unload();
                    Load();
                }
                if (PlaybackEvent != null)
                    PlaybackEvent(this, new PlaybackEventArgs(Status, NowPlayingId, NowPlaying.FilePath));
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
                if (Status == PlaybackStatus.Stopped)
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
            if (NowPlayingId > 0)
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
            Unload();
            Load();
            _myWaveOut.Play();
            Status = PlaybackStatus.Playing;
            if (PlaybackEvent != null)
                PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Playing, NowPlayingId, NowPlaying.FilePath));
        }

        public void Pause()
        {
            _myWaveOut.Pause();
            Status = PlaybackStatus.Paused;
            if (PlaybackEvent != null)
                PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Paused, NowPlayingId, NowPlaying.FilePath));
        }

        public void Resume()
        {
            if (Status == PlaybackStatus.Paused)
            {
                _myWaveOut.Play();
                Status = PlaybackStatus.Playing;
                if (PlaybackEvent != null)
                    PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Resumed, NowPlayingId, NowPlaying.FilePath));
            }
            else // Dummy
                _myWaveOut.Play();
        }

        public void Stop()
        {
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            Status = PlaybackStatus.Stopped;
            if (PlaybackEvent != null)
                PlaybackEvent(this, new PlaybackEventArgs(PlaybackStatus.Stopped, NowPlayingId, NowPlaying.FilePath));
        }

        public void Load()
        {
            _myWaveOut = new DirectSoundOut();

            try
            {
                _myAudioFileReader = Activator.CreateInstance((from c in codecs
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
