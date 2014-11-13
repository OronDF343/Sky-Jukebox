﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using SkyJukebox.Api;
using SkyJukebox.Core.Xml;

namespace SkyJukebox.Core.Playback
{
    public sealed class PlaybackManager : IPlaybackManager, IDisposable
    {
        #region Properties and Fields
        private IAudioPlayer _currentPlayer;
        public IPlaylist Playlist { get; set; }
        private int _nowPlayingId;
        public int NowPlayingId
        {
            get { return _nowPlayingId; }
            set
            {
                _nowPlayingId = value;
                _currentState.OnSongChange(this);
            }
        }
        public IMusicInfo NowPlaying { get { return Playlist[NowPlayingId]; } }
        public float Volume
        {
            get { return _currentPlayer.Volume; }
            set { _currentPlayer.Volume = value; }
        }

        public float Balance
        {
            get { return _currentPlayer.Balance; }
            set { _currentPlayer.Balance = value; }
        }
        public TimeSpan Position
        {
            get { return _currentPlayer.Position; }
            set { _currentPlayer.Position = value; }
        }
        public TimeSpan Duration
        {
            get { return _currentPlayer.Duration; }
        }

        private bool _shuffle;
        public bool Shuffle
        {
            get { return _shuffle; }
            set
            {
                _shuffle = value;
                Playlist.ShuffleIndex = value;
                _currentState.Shuffle(this, value);
            }
        }
        public bool AutoPlay { get; set; }
        public LoopTypes LoopType { get; set; }
        #endregion

        #region State property
        public enum PlaybackStates { Stopped = 0, Paused = 1, Playing = 2 }

        private IState _currentState;
        public PlaybackStates CurrentState { get; private set; }
        private readonly IState[] _states;

        private void SetState(PlaybackStates ps)
        {
            _currentState = _states[Convert.ToInt32(ps)];
            CurrentState = ps;
        }

        private interface IState
        {
            void PlayPauseResume(PlaybackManager pm);
            void OnSongChange(PlaybackManager pm);
            void Stop(PlaybackManager pm);
            void Shuffle(PlaybackManager pm, bool t);
        }

        private class Stopped : IState
        {
            public void PlayPauseResume(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Playing);
                pm.Play();
            }

            public void OnSongChange(PlaybackManager pm)
            {
                pm.Load();
            }

            public void Stop(PlaybackManager pm) { }

            public void Shuffle(PlaybackManager pm, bool t)
            {
                pm.NowPlayingId = 0;
                pm.Load();
            }
        }

        private class Paused : IState
        {
            public void PlayPauseResume(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Playing);
                pm.Resume();
            }

            public void OnSongChange(PlaybackManager pm)
            {
                if (!pm.Load()) return;
                pm.SetState(PlaybackStates.Playing);
                pm.Play();
            }

            public void Stop(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Stopped);
                pm._Stop();
            }

            public void Shuffle(PlaybackManager pm, bool t)
            {
                pm.NowPlayingId = 0;
                if (!pm.Load()) return;
                pm.SetState(PlaybackStates.Playing);
                pm.Play();
            }
        }

        private class Playing : IState
        {
            public void PlayPauseResume(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Paused);
                pm.Pause();
            }

            public void OnSongChange(PlaybackManager pm)
            {
                if (!pm.Load()) return;
                pm.Play();
            }

            public void Stop(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Stopped);
                pm._Stop();
            }

            public void Shuffle(PlaybackManager pm, bool t) { }
        }
        #endregion

        #region Singleton
        private PlaybackManager()
        {
            Playlist = new Playlist();
            AutoPlay = true;
            LoopType = LoopTypes.None;
            _states = new IState[] { new Stopped(), new Paused(), new Playing() };
            SetState(PlaybackStates.Stopped);
            _nowPlayingId = 0;
            _playbackTimer.Tick += PlaybackTimerOnTick;
        }

        private static PlaybackManager _instance;
        public static PlaybackManager Instance { get { return _instance ?? (_instance = new PlaybackManager()); } }
        #endregion

        #region Players registry
        private readonly Dictionary<IEnumerable<string>, IAudioPlayer> _audioPlayers = new Dictionary<IEnumerable<string>, IAudioPlayer>();
        public void RegisterAudioPlayer(IEnumerable<string> extensions, IAudioPlayer player)
        {
            _audioPlayers.Add(extensions, player);
        }

        public bool HasSupportingPlayer(string extension)
        {
            return _audioPlayers.Keys.Count(e => e.Contains(extension.ToLowerInvariant())) > 0;
        }

        public Dictionary<string, IEnumerable<string>> GetAudioPlayerInfo()
        {
            return _audioPlayers.ToDictionary(g => g.Value.GetType().FullName, g => g.Key);
        }
        #endregion

        #region Playback control
        private bool? _lastLoadSucess;

        private bool Load()
        {
            Unload();
            if (NowPlayingId >= Playlist.Count || NowPlayingId < 0)
            {
                _lastLoadSucess = null;
                return false;
            }
            _currentPlayer = (from c in _audioPlayers
                              where c.Key.Contains(NowPlaying.Extension)
                              select c.Value).First();
            if (_currentPlayer == null) throw new NullReferenceException("Failed to create IAudioPlayer! Invalid or missing codec!");
            _currentPlayer.PlaybackFinished += CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError += CurrentPlayerOnPlaybackError;
            return (bool)(_lastLoadSucess = _currentPlayer.Load(NowPlaying.FilePath, Settings.Instance.PlaybackDevice));
        }

        private void Unload()
        {
            _playbackTimer.IsEnabled = false;
            if (_currentPlayer == null) return;
            _currentPlayer.PlaybackFinished -= CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError -= CurrentPlayerOnPlaybackError;
            TimerTickEvent(this, new TimerTickEventArgs(new TimeSpan(0), Duration));
            _currentPlayer.Unload();
        }

        private void CurrentPlayerOnPlaybackError(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void CurrentPlayerOnPlaybackFinished(object sender, EventArgs eventArgs)
        {
            _playbackTimer.IsEnabled = false;
            TimerTickEvent(this, new TimerTickEventArgs(new TimeSpan(0), Duration));

            if (!AutoPlay)
                SetState(PlaybackStates.Stopped);

            if (LoopType == LoopTypes.Single)
                PlayPauseResume();
            else
                Next();
        }

        public void Next()
        {
            if (Playlist.Count == 0) return;

            if (NowPlayingId < Playlist.Count - 1)
                ++NowPlayingId;
            else
            {
                if (LoopType == LoopTypes.None)
                    SetState(PlaybackStates.Stopped);
                NowPlayingId = 0;
            }
            FirePlaybackEvent();
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
                if (LoopType == LoopTypes.None)
                    SetState(PlaybackStates.Stopped);
                NowPlayingId = Playlist.Count - 1;
            }
            FirePlaybackEvent();
        }

        public void PlayPauseResume()
        {
            if (_lastLoadSucess == null) if (!Load()) return;
            _currentState.PlayPauseResume(this);
        }

        private void Play()
        {
            if (_lastLoadSucess != true) return;
            _currentPlayer.Play();
            _playbackTimer.IsEnabled = true;
            FirePlaybackEvent();
        }

        private void Pause()
        {
            if (_lastLoadSucess != true) return;
            _currentPlayer.Pause();
            _playbackTimer.IsEnabled = false;
            FirePlaybackEvent();
        }

        private void Resume()
        {
            if (_lastLoadSucess != true) return;
            _currentPlayer.Resume();
            _playbackTimer.IsEnabled = true;
            FirePlaybackEvent();
        }

        private void _Stop()
        {
            if (_lastLoadSucess != true) return;
            _playbackTimer.IsEnabled = false;
            _currentPlayer.Stop();
            TimerTickEvent(this, new TimerTickEventArgs(new TimeSpan(0), Duration));
            FirePlaybackEvent();
        }

        public void Stop()
        {
            _currentState.Stop(this);
        }
        #endregion

        #region Events
        public event EventHandler<PlaybackEventArgs> PlaybackEvent;
        public class PlaybackEventArgs : EventArgs
        {
            public bool IsError { get; set; }
            public string NewTrackName { get; set; }
            public int NewTrackId { get; set; }
            public PlaybackStates NewState { get; set; }
            public string Message { get; set; }
            public PlaybackEventArgs(PlaybackStates state, int newTrackId, string newTrackName, bool isError = false, string msg = "")
            {
                NewState = state;
                NewTrackId = newTrackId;
                NewTrackName = newTrackName;
                IsError = isError;
                Message = msg;
            }
        }
        private void FirePlaybackEvent()
        {
            if (PlaybackEvent == null) return;
            if (NowPlayingId < Playlist.Count && NowPlayingId >= 0)
                PlaybackEvent(this, new PlaybackEventArgs(CurrentState, NowPlayingId, NowPlaying.FilePath));
            else
                PlaybackEvent(this, new PlaybackEventArgs(CurrentState, NowPlayingId, "[missing]", false, "[not found in playlist]"));
        }


        readonly DispatcherTimer _playbackTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 40), IsEnabled = false };
        public event EventHandler<TimerTickEventArgs> TimerTickEvent;
        public class TimerTickEventArgs : EventArgs
        {
            public TimeSpan Elapsed { get; set; }
            public TimeSpan Duration { get; set; }

            public TimerTickEventArgs(TimeSpan e, TimeSpan d)
            {
                Elapsed = e;
                Duration = d;
            }
        }


        private void PlaybackTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (TimerTickEvent != null)
                TimerTickEvent(this, new TimerTickEventArgs(Position, Duration));
        }
        #endregion

        #region Disposal
        public void Dispose()
        {
            Unload();
            foreach (var audioPlayer in _audioPlayers)
            {
                if (audioPlayer.Value != null)
                    audioPlayer.Value.Dispose(); 
            }
        }
        #endregion
    }
}