using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Flac;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using NAudio.Wave;
using SkyJukebox.Api.Playback;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.NAudioFramework
{
    public sealed class NAudioPlayer : IAudioPlayer
    {
        public string ExtensionId { get { return "NAudioPlayer"; } }
        internal void Init()
        {
            // Load built-in NAudio codecs
            AddCodec(new string[] { "mp3", "wav", "m4a", "aac", "aiff", "mpc", "ape" }, typeof(AudioFileReader));
            AddCodec(new string[] { "wma" }, typeof(WMAFileReader));
            AddCodec(new string[] { "ogg" }, typeof(VorbisWaveReader));
            AddCodec(new string[] { "flac" }, typeof(FlacReader));

            // Load external NAudio codecs
            foreach (var c in ExtensionLoader.GetExtensions<ICodec>(PathStringUtils.GetExePath()))
            {
                if (!c.WaveStreamType.IsSubclassOf(typeof(WaveStream)))
                    throw new InvalidOperationException("A plugin tried to register an NAudio codec which doesn't derive from WaveStream!");
                var e = from x in c.Extensions
                        select x.ToLower();
                AddCodec(e, c.WaveStreamType);
            }
        }

        private readonly Dictionary<IEnumerable<string>, Type> _codecs = new Dictionary<IEnumerable<string>, Type>();

        private void AddCodec(IEnumerable<string> exts, Type t)
        {
            _codecs.Add(exts, t);
        }
        public bool HasCodec(string ext)
        {
            return _codecs.Keys.Count(es => es.Contains(ext.ToLowerInvariant())) > 0;
        }

        public Dictionary<string, IEnumerable<string>> GetCodecInfo()
        {
            return _codecs.ToDictionary(g => g.Value.FullName, g => g.Key);
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
                _myWaveStream = Activator.CreateInstance(_codecs[_codecs.Keys.First(k => k.Contains(cext))], path) as WaveStream;
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
                   from s in c.Key
                   select s;
        }


        public IEnumerable<string> Extensions
        {
            get { return GetCodecs(); }
        }

        public bool IsSomethingLoaded { get { return _myWaveStream != null; } }
    }
}
