using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAudio.Flac;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using NAudio.Wave;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.NAudioFramework
{
    [Extension("NAudioPlayer", "1.0.0.0", "1.0.0.0", "Plays audio using the NAudio library.")]
    public sealed class NAudioPlayer : IAudioPlayer
    {
        static NAudioPlayer()
        {
            var epath = Assembly.GetExecutingAssembly().Location;
            var exePath = epath.Substring(0, epath.LastIndexOf('\\') + 1);
            // Load built-in NAudio codecs
            AddCodec(new string[] { "mp3", "wav", "m4a", "aac", "aiff", "mpc", "ape" }, typeof(AudioFileReader));
            AddCodec(new string[] { "wma" }, typeof(WMAFileReader));
            AddCodec(new string[] { "ogg" }, typeof(VorbisWaveReader));
            AddCodec(new string[] { "flac" }, typeof(FlacReader));

            // Load external NAudio codecs
            foreach (var c in ExtensionLoader.GetExtensions<ICodec>(exePath))
            {
                if (!c.WaveStreamType.IsSubclassOf(typeof(WaveStream)))
                    throw new InvalidOperationException("A plugin tried to register an NAudio codec which doesn't derive from WaveStream!");
                var e = from x in c.Extensions
                        select x.ToLower();
                AddCodec(e, c.WaveStreamType);
            }
        }

        private static readonly Dictionary<IEnumerable<string>, Type> Codecs = new Dictionary<IEnumerable<string>, Type>();

        private static void AddCodec(IEnumerable<string> exts, Type t)
        {
            Codecs.Add(exts, t);
        }
        public static bool HasCodec(string ext)
        {
            return Codecs.Keys.Count(es => es.Contains(ext.ToLowerInvariant())) > 0;
        }

        public static Dictionary<string, IEnumerable<string>> GetCodecInfo()
        {
            return Codecs.ToDictionary(g => g.Value.FullName, g => g.Key);
        }

        private IWavePlayer _myWaveOut;
        private WaveChannel32 _myAudioFileReader;

        public event EventHandler PlaybackFinished;

        public event EventHandler PlaybackError;

        public bool Load(string path, Guid device)
        {
            var cext = path.GetExt();
            _myWaveOut = new DirectSoundOut(device);
            WaveStream afr;
            try
            {
                afr = Activator.CreateInstance((from c in Codecs
                                                where c.Key.Contains(cext)
                                                select c.Value).First(), path) as WaveStream;
            }
            catch
            {
                return false;
            }
            if (afr == null) return false;
            _myAudioFileReader = new WaveChannel32(afr) { PadWithZeroes = false };
            _myWaveOut.Init(_myAudioFileReader);
            _myWaveOut.PlaybackStopped += MyWaveOutOnPlaybackStopped;
            _myAudioFileReader.Volume = (float)Volume;
            _myAudioFileReader.Pan = (float)Balance;
            return true;
        }

        private bool _stopped = true;
        private void MyWaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (_stopped) return;
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
            if (_myAudioFileReader == null) return;
            _myAudioFileReader.Dispose();
            _myAudioFileReader = null;
        }

        public void Play()
        {
            Stop();
            _stopped = false;
            _myWaveOut.Play();
        }

        public void Pause()
        {
            _stopped = true;
            _myWaveOut.Pause();
        }

        public void Resume()
        {
            _stopped = false;
            _myWaveOut.Play();
        }

        public void Stop()
        {
            _stopped = true;
            if (_myWaveOut != null)
                _myWaveOut.Stop();
            _myAudioFileReader.Position = 0;
        }

        private decimal _volume = 1.0m;
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_myAudioFileReader != null)
                    _myAudioFileReader.Volume = (float)_volume;
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                if (_myAudioFileReader != null)
                    _myAudioFileReader.Pan = (float)_balance;
            }
        }

        public TimeSpan Duration
        {
            get { return _myAudioFileReader != null ? _myAudioFileReader.TotalTime : new TimeSpan(); }
        }

        public TimeSpan Position
        {
            get { return _myAudioFileReader != null ? _myAudioFileReader.CurrentTime : new TimeSpan(); }
            set { if (_myAudioFileReader != null) _myAudioFileReader.CurrentTime = value; }
        }

        public void Dispose()
        {
            Unload();
            if (_myWaveOut == null) return;
            _myWaveOut.Dispose();
            _myWaveOut = null;
        }


        private static IEnumerable<string> GetCodecs()
        {
            return from c in Codecs
                   from s in c.Key
                   select s;
        }


        public IEnumerable<string> Extensions
        {
            get { return GetCodecs(); }
        }

        public TimeSpan GetDuration(string file)
        {
            try
            {
                var co = Activator.CreateInstance((from c in Codecs
                                                   where c.Key.Contains(file.GetExt())
                                                   select c.Value).First(), file) as WaveStream;
                if (co == null)
                    return TimeSpan.Zero;
                var d = co.TotalTime;
                co.Dispose();
                return d;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
        public long GetLength(string file)
        {
            try
            {
                var co = Activator.CreateInstance((from c in Codecs
                                                   where c.Key.Contains(file.GetExt())
                                                   select c.Value).First(), file) as WaveStream;
                if (co == null)
                    return 0;
                var d = co.Length;
                co.Dispose();
                return d;
            }
            catch
            {
                return 0;
            }
        }
    }
}
