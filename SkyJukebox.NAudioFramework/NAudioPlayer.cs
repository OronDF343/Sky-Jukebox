using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
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
            // Load built-in codecs
            AddCodec(new CoreCodec());
            AddCodec(new WmaCodec());
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
        private WaveChannel32 _myWaveChannel32;

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
            _myWaveChannel32 = new WaveChannel32(_myWaveStream) { PadWithZeroes = false };
            _myWaveOut.Init(_myWaveChannel32);
            _myWaveOut.PlaybackStopped += MyWaveOutOnPlaybackStopped;
            _myWaveChannel32.Volume = (float)Volume;
            _myWaveChannel32.Pan = (float)Balance;
            return true;
        }

        private volatile bool _userStopped = true;
        private void MyWaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (_userStopped) return;
            if (stoppedEventArgs.Exception == null && PlaybackFinished != null)
                PlaybackFinished(this, new EventArgs());
            else if (PlaybackError != null)
                PlaybackError(this, new EventArgs());
        }

        public void Unload()
        {
            if (_myWaveOut != null)
            {
                _myWaveOut.PlaybackStopped -= MyWaveOutOnPlaybackStopped;
                _myWaveOut.Stop();
            }
            if (_myWaveChannel32 != null)
            {
                _myWaveChannel32.Dispose();
                _myWaveChannel32 = null;
            }
            _myWaveStream = null;
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
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            _myWaveStream.CurrentTime = TimeSpan.Zero;
        }

        private decimal _volume = 1.0m;
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_myWaveChannel32 != null)
                    _myWaveChannel32.Volume = (float)_volume;
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                if (_myWaveChannel32 != null)
                    _myWaveChannel32.Pan = (float)_balance;
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
    }
}
