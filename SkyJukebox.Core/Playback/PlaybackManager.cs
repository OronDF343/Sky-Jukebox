using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using SkyJukebox.Api;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib;

namespace SkyJukebox.Core.Playback
{
    public sealed class PlaybackManager : IPlaybackManager, IDisposable
    {
        #region Properties and Fields
        private IAudioPlayer _currentPlayer;
        public IPlaylist Playlist { get; set; } // has own notifications
        private int _nowPlayingId;
        public int NowPlayingId
        {
            get { return _nowPlayingId; }
            set
            {
                _nowPlayingId = value;
                OnPropertyChanged("NowPlayingId");
                OnPropertyChanged("NowPlaying");
                _currentState.OnSongChange(this);
            }
        }
        public IMusicInfo NowPlaying { get { return Playlist[NowPlayingId]; } }

        private decimal _volume = 1.0m;
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_currentPlayer != null)
                    _currentPlayer.Volume = _volume;
                OnPropertyChanged("Volume");
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                if(_currentPlayer != null)
                    _currentPlayer.Balance = _balance;
                OnPropertyChanged("Balance");
            }
        }
        public TimeSpan Position // notifies based on timer
        {
            get { return _currentPlayer.Position; }
            set
            {
                _currentPlayer.Position = value;
                OnPropertyChanged("Position");
            }
        }
        public TimeSpan Duration // notifies on load
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
                OnPropertyChanged("Shuffle");
                _currentState.Shuffle(this, value);
            }
        }

        private bool _autoPlay;
        public bool AutoPlay
        {
            get { return _autoPlay; }
            set
            {
                _autoPlay = value;
                OnPropertyChanged("AutoPlay");
            }
        }

        private LoopTypes _loopType;
        public LoopTypes LoopType
        {
            get { return _loopType; }
            set
            {
                _loopType = value;
                OnPropertyChanged("LoopType");
            }
        }
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
            OnPropertyChanged("CurrentState");
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
                if (pm.Playlist.Count > 0)
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
            Playlist.CollectionChanged += Playlist_CollectionChanged;
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
            _currentPlayer.Volume = Volume;
            _currentPlayer.Balance = Balance;
            OnPropertyChanged("Duration");
            return (bool)(_lastLoadSucess = _currentPlayer.Load(NowPlaying.FilePath, Settings.Instance.PlaybackDevice));
        }

        private void Unload()
        {
            _playbackTimer.IsEnabled = false;
            if (_currentPlayer == null) return;
            _currentPlayer.PlaybackFinished -= CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError -= CurrentPlayerOnPlaybackError;
            OnPropertyChanged("Position");
            _currentPlayer.Unload();
        }

        private void CurrentPlayerOnPlaybackError(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void CurrentPlayerOnPlaybackFinished(object sender, EventArgs eventArgs)
        {
            _playbackTimer.IsEnabled = false;
            OnPropertyChanged("Position");

            if (LoopType == LoopTypes.Single)
            {
                SetState(PlaybackStates.Stopped);
                PlayPauseResume();
            }
            else
            {
                if (!AutoPlay)
                    SetState(PlaybackStates.Stopped);
                Next();
            }
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
        }

        private void Pause()
        {
            if (_lastLoadSucess != true) return;
            _currentPlayer.Pause();
            _playbackTimer.IsEnabled = false;
        }

        private void Resume()
        {
            if (_lastLoadSucess != true) return;
            _currentPlayer.Resume();
            _playbackTimer.IsEnabled = true;
        }

        private void _Stop()
        {
            if (_lastLoadSucess != true) return;
            _playbackTimer.IsEnabled = false;
            _currentPlayer.Stop();
            OnPropertyChanged("Position");
        }

        public void Stop()
        {
            _currentState.Stop(this);
        }

        public void Reset()
        {
            Stop();
            Load();
        }
        #endregion

        public TimeSpan GetDuration(string file)
        {
            var player = (from c in _audioPlayers
                          where c.Key.Contains(file.GetExt())
                          select c.Value).First();
            return player.GetDuration(file);
        }

        public long GetLength(string file)
        {
            var player = (from c in _audioPlayers
                          where c.Key.Contains(file.GetExt())
                          select c.Value).First();
            return player.GetLength(file);
        }

        #region Playback Timer

        readonly DispatcherTimer _playbackTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 40), IsEnabled = false };
        private void PlaybackTimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Position");
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

        private void Playlist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ((e.NewStartingIndex == NowPlayingId || e.OldStartingIndex == NowPlayingId) && NowPlayingId < Playlist.Count)
                OnPropertyChanged("NowPlaying");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
