using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                OnPropertyChanged("AbsoluteNowPlayingId");
                OnPropertyChanged("NowPlaying");
                LoadedTrack = NowPlaying;
                _currentState.OnSongChange(this);
            }
        }

        public int AbsoluteNowPlayingId
        {
            get { return Playlist.AbsoluteIndexOf(_nowPlayingId); }
            set { NowPlayingId = Playlist.ShuffledIndexOf(value); }
        }

        public IMusicInfo NowPlaying { get { return Playlist[NowPlayingId]; } }
        public IMusicInfo LoadedTrack { get; private set; }

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
            get { return _currentPlayer != null ? _currentPlayer.Position : TimeSpan.Zero; }
            set
            {
                _currentPlayer.Position = value;
                OnPropertyChanged("Position");
            }
        }

        public TimeSpan Duration // notifies on load
        {
            get { return _currentPlayer != null ? _currentPlayer.Duration : TimeSpan.Zero; }
        }

        public bool Shuffle
        {
            get { return Playlist.ShuffleIndex; }
            set
            {
                Playlist.ShuffleIndex = value;
                OnPropertyChanged("Shuffle");
                if (Playlist.Count < 1) return;
                _nowPlayingId = Playlist.ShuffledIndexOf(LoadedTrack);
                if (CurrentState != PlaybackStates.Playing && (_nowPlayingId < 0 || _nowPlayingId >= Playlist.Count))
                    NowPlayingId = 0;
                OnPropertyChanged("NowPlayingId");
                OnPropertyChanged("AbsoluteNowPlayingId");
            }
        }

        private bool _autoPlay = true;
        public bool AutoPlay
        {
            get { return _autoPlay; }
            set
            {
                _autoPlay = value;
                OnPropertyChanged("AutoPlay");
            }
        }

        private LoopTypes _loopType = LoopTypes.None;
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
                pm.SetState(PlaybackStates.Stopped);
                pm.Load();
            }

            public void Stop(PlaybackManager pm)
            {
                pm.SetState(PlaybackStates.Stopped);
                pm._Stop();
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
        }
        #endregion

        #region Singleton
        private PlaybackManager()
        {
            Playlist = new Playlist();
            _states = new IState[] { new Stopped(), new Paused(), new Playing() };
            SetState(PlaybackStates.Stopped);
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

        private IEnumerable<string> _supported;
        // TODO: ObservableCollections
        public IEnumerable<string> SupportedFileTypes { get { return _supported ?? (_supported = from es in GetAudioPlayerInfo().Values from s in es select s); } }
        #endregion

        #region Playback control

        private bool? _lastLoadSucess;

        public bool IsSomethingLoaded
        {
            get { return _currentPlayer != null && _currentPlayer.IsSomethingLoaded && _lastLoadSucess == true; }
        }

        private bool Load()
        {
            if (IsSomethingLoaded) Unload();
            if (NowPlayingId >= Playlist.Count || NowPlayingId < 0)
            {
                _lastLoadSucess = null;
                return false;
            }

            var gotValue = false;
            try
            {
                gotValue = _audioPlayers.TryGetValue(_audioPlayers.Keys.First(k => k.Contains(NowPlaying.Extension)), out _currentPlayer);
            }
            finally
            {
                if (_currentPlayer == null || gotValue == false) throw new NullReferenceException("Failed to create IAudioPlayer! Invalid or missing codec!");
            }

            _lastLoadSucess = _currentPlayer.Load(NowPlaying.FilePath, (Guid)SettingsManager.Instance["PlaybackDevice"].Value);
            if (_lastLoadSucess != true) return false;
            OnPropertyChanged("Duration");
            _currentPlayer.PlaybackFinished += CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError += CurrentPlayerOnPlaybackError;
            _currentPlayer.Volume = Volume;
            _currentPlayer.Balance = Balance;
            return true;
        }

        private void Unload()
        {
            _playbackTimer.IsEnabled = false;
            if (_currentPlayer == null) return;
            _currentPlayer.PlaybackFinished -= CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError -= CurrentPlayerOnPlaybackError;
            _currentPlayer.Unload();
            OnPropertyChanged("Position");
            OnPropertyChanged("Duration");
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
            if (Playlist.Count < 1) return;

            if (NowPlayingId < Playlist.Count - 1)
                ++NowPlayingId;
            else
            {
                if (LoopType != LoopTypes.All)
                    SetState(PlaybackStates.Stopped);
                NowPlayingId = 0;
            }
        }

        public void Previous()
        {
            if (Playlist.Count < 1) return;

            if (NowPlayingId >= Playlist.Count)
                NowPlayingId = Playlist.Count - 1;
            else if (NowPlayingId > 0)
                --NowPlayingId;
            else
            {
                if (LoopType != LoopTypes.All)
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
            if (!IsSomethingLoaded) return;
            _currentPlayer.Play();
            _playbackTimer.IsEnabled = true;
        }

        private void Pause()
        {
            if (!IsSomethingLoaded) return;
            _playbackTimer.IsEnabled = false;
            _currentPlayer.Pause();
            OnPropertyChanged("Position");
        }

        private void Resume()
        {
            if (!IsSomethingLoaded) return;
            _currentPlayer.Resume();
            _playbackTimer.IsEnabled = true;
        }

        private void _Stop()
        {
            if (!IsSomethingLoaded) return;
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
            IAudioPlayer player;
            bool gotValue;
            try
            {
                gotValue = _audioPlayers.TryGetValue(_audioPlayers.Keys.First(k => k.Contains(file.GetExt())), out player);
            }
            catch
            {
                return TimeSpan.Zero;
            }
            return gotValue ? player.GetDuration(file) : TimeSpan.Zero;
        }

        public long GetLength(string file)
        {
            IAudioPlayer player;
            bool gotValue;
            try
            {
                gotValue = _audioPlayers.TryGetValue(_audioPlayers.Keys.First(k => k.Contains(file.GetExt())), out player);
            }
            catch
            {
                return 0;
            }
            return gotValue ? player.GetLength(file) : 0;
        }

        #region Playback Timer

        readonly DispatcherTimer _playbackTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50), IsEnabled = false };
        private void PlaybackTimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Position");
        }
        #endregion

        #region Disposal
        public void Dispose()
        {
            Unload();
            foreach (var audioPlayer in _audioPlayers.Values.Where(ap => ap != null))
                audioPlayer.Dispose();
        }

        #endregion

        private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Playlist.Count < 1) return;
            _nowPlayingId = Playlist.ShuffledIndexOf(LoadedTrack);
            if (CurrentState != PlaybackStates.Playing && (_nowPlayingId < 0 || _nowPlayingId >= Playlist.Count))
                NowPlayingId = 0;
            OnPropertyChanged("NowPlayingId");
            OnPropertyChanged("AbsoluteNowPlayingId");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
