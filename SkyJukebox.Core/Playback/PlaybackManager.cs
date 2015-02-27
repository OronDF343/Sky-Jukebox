using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using SkyJukebox.Api.Playback;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Core.Xml;

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
                SettingsManager.Instance["Volume"].Value = value;
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
                SettingsManager.Instance["Balance"].Value = value;
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
                if (CurrentState != PlaybackState.Playing && (_nowPlayingId < 0 || _nowPlayingId >= Playlist.Count))
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

        private IState _currentState;

        public PlaybackState CurrentState { get; private set; }

        private readonly IState[] _states;

        private void SetState(PlaybackState ps)
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
                pm.SetState(PlaybackState.Playing);
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
                pm.SetState(PlaybackState.Playing);
                pm.Resume();
            }

            public void OnSongChange(PlaybackManager pm)
            {
                pm.SetState(PlaybackState.Stopped);
                pm.Load();
            }

            public void Stop(PlaybackManager pm)
            {
                pm.SetState(PlaybackState.Stopped);
                pm._Stop();
            }
        }

        private class Playing : IState
        {
            public void PlayPauseResume(PlaybackManager pm)
            {
                pm.SetState(PlaybackState.Paused);
                pm.Pause();
            }

            public void OnSongChange(PlaybackManager pm)
            {
                if (!pm.Load()) return;
                pm.Play();
            }

            public void Stop(PlaybackManager pm)
            {
                pm.SetState(PlaybackState.Stopped);
                pm._Stop();
            }
        }
        #endregion

        #region Singleton
        private PlaybackManager()
        {
            Playlist = new Playlist.Playlist();
            _states = new IState[] { new Stopped(), new Paused(), new Playing() };
            SetState(PlaybackState.Stopped);
            _playbackTimer.Tick += PlaybackTimerOnTick;
            Playlist.CollectionChanged += Playlist_CollectionChanged;
            Balance = (decimal)SettingsManager.Instance["Balance"].Value;
            Volume = (decimal)SettingsManager.Instance["Volume"].Value;
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

        private ICollection<string> _supported;
        // TODO: ObservableCollections
        public ICollection<string> SupportedFileTypes { get { return _supported ?? (_supported = (from es in GetAudioPlayerInfo().Values from s in es select s).ToList()); } }
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
            if (_currentPlayer is INotifyPropertyChanged) (_currentPlayer as INotifyPropertyChanged).PropertyChanged += CurrentPlayer_PropertyChanged;
            _currentPlayer.Volume = Volume;
            _currentPlayer.Balance = Balance;
            return true;
        }

        private void CurrentPlayer_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OnPropertyChanged(propertyChangedEventArgs.PropertyName);
        }

        private void Unload()
        {
            _playbackTimer.IsEnabled = false;
            if (_currentPlayer == null) return;
            _currentPlayer.PlaybackFinished -= CurrentPlayerOnPlaybackFinished;
            _currentPlayer.PlaybackError -= CurrentPlayerOnPlaybackError;
            if (_currentPlayer is INotifyPropertyChanged) (_currentPlayer as INotifyPropertyChanged).PropertyChanged -= CurrentPlayer_PropertyChanged;
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
                SetState(PlaybackState.Stopped);
                PlayPauseResume();
            }
            else
            {
                if (!AutoPlay)
                    SetState(PlaybackState.Stopped);
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
                    SetState(PlaybackState.Stopped);
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
                    SetState(PlaybackState.Stopped);
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
            if (CurrentState != PlaybackState.Playing && (_nowPlayingId < 0 || _nowPlayingId >= Playlist.Count))
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
