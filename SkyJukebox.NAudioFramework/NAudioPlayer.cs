using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SkyJukebox.Api.Playback;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;
using SkyJukebox.NAudioFramework.Codecs;

namespace SkyJukebox.NAudioFramework
{
    public sealed class NAudioPlayer : IAudioPlayer
    {
        public string ExtensionId { get { return "NAudioPlayer"; } }
        internal void Init()
        {
            _equalizerBands = new ObservableCollection<IEqualizerBand>();

            // Load built-in codecs
            AddCodec(new CoreCodec());
            AddCodec(new VorbisCodec());
            AddCodec(new FlacCodec());

            // Load external codecs
            _codecs.AddRange(ExtensionLoader.GetExtensions<ICodec>(PathStringUtils.GetExePath()));
        }

        private readonly List<ICodec> _codecs = new List<ICodec>();

        public void AddCodec(ICodec t)
        {
            _codecs.Add(t);
        }
        public bool HasCodec(string ext)
        {
            return _codecs.Any(es => es.Extensions.Contains(ext.ToLowerInvariant()));
        }

        public Dictionary<string, IEnumerable<string>> GetCodecInfo()
        {
            return _codecs.ToDictionary(g => g.Name, g => g.Extensions);
        }

        private IWavePlayer _myWaveOut;
        private WaveStream _myWaveStream;
        private BalanceSampleProvider _myBalanceSampleProvider;
        private VolumeSampleProvider _myVolumeSampleProvider;
        private Equalizer _myEqualizer;

        public event EventHandler PlaybackFinished;

        public event EventHandler PlaybackError;

        public bool Load(string path, Guid device)
        {
            var cext = path.GetExt();
            _myWaveOut = new DirectSoundOut(device);
            try
            {
                _myWaveStream = _codecs.First(v => v.Extensions.Contains(cext)).CreateWaveStream(path);
            }
            catch
            {
                return false;
            }
            if (_myWaveStream == null) return false;
            if (_myWaveStream.WaveFormat.Channels == 2)
            {
                _myBalanceSampleProvider = new BalanceSampleProvider(_myWaveStream.ToSampleProvider());
                _myVolumeSampleProvider = new VolumeSampleProvider(_myBalanceSampleProvider);
                _myBalanceSampleProvider.Pan = (float)Balance;
            }
            else _myVolumeSampleProvider = new VolumeSampleProvider(_myWaveStream.ToSampleProvider());
            _myEqualizer = new Equalizer(_myVolumeSampleProvider, _equalizerBands) { Enabled = _enableEqualizer };
            _myWaveOut.Init(_myEqualizer);
            _myWaveOut.PlaybackStopped += MyWaveOutOnPlaybackStopped;
            _myVolumeSampleProvider.Volume = (float)Volume;
            return true;
        }

        private volatile bool _userStopped = true;
        private void MyWaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (_userStopped) return;
            if (stoppedEventArgs.Exception == null && PlaybackFinished != null) PlaybackFinished(this, new EventArgs());
            else if (PlaybackError != null) PlaybackError(this, new EventArgs());
        }

        public void Unload()
        {
            if (_myWaveOut != null)
            {
                _myWaveOut.PlaybackStopped -= MyWaveOutOnPlaybackStopped;
                _myWaveOut.Stop();
            }
            if (_myWaveStream != null)
            {
                _myWaveStream.Dispose();
                _myWaveStream = null;
            }
            _myBalanceSampleProvider = null;
            _myVolumeSampleProvider = null;
            _myEqualizer = null;
        }

        public void Play()
        {
            _userStopped = false;
            _myWaveOut.Play();
        }

        public void Pause()
        {
            _userStopped = true;
            _myWaveOut.Pause();
        }

        public void Resume()
        {
            _userStopped = false;
            _myWaveOut.Play();
        }

        public void Stop()
        {
            _userStopped = true;
            if (_myWaveOut != null) _myWaveOut.Stop();
            _myWaveStream.CurrentTime = TimeSpan.Zero;
        }

        private decimal _volume = 1.0m;
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_myVolumeSampleProvider != null) _myVolumeSampleProvider.Volume = (float)_volume;
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                if (_myBalanceSampleProvider != null) _myBalanceSampleProvider.Pan = (float)_balance;
            }
        }

        public TimeSpan Duration
        {
            get { return _myWaveStream != null ? _myWaveStream.TotalTime : new TimeSpan(); }
        }

        public TimeSpan Position
        {
            get { return _myWaveStream != null ? _myWaveStream.CurrentTime : new TimeSpan(); }
            set { if (_myWaveStream != null) _myWaveStream.CurrentTime = value; }
        }

        public void Dispose()
        {
            Unload();
            if (_myWaveOut == null) return;
            _myWaveOut.Dispose();
            _myWaveOut = null;
        }


        private IEnumerable<string> GetCodecs()
        {
            return from c in _codecs
                   from s in c.Extensions
                   select s;
        }


        public IEnumerable<string> Extensions
        {
            get { return GetCodecs(); }
        }

        public bool IsSomethingLoaded { get { return _myWaveStream != null; } }

        #region EQ

        private bool _enableEqualizer;

        public bool EnableEqualizer
        {
            get { return _enableEqualizer; }
            set
            {
                _enableEqualizer = value;
                if (_myEqualizer != null) _myEqualizer.Enabled = value;
            }
        }

        private ObservableCollection<IEqualizerBand> _equalizerBands;

        public ObservableCollection<IEqualizerBand> EqualizerBands { get { return _equalizerBands; } }

        #endregion
    }
}
