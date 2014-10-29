using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using SkyJukebox.CoreApi;
using SkyJukebox.CoreApi.Contracts;
using SkyJukebox.CoreApi.Utils;
using NAudio.Wave;

namespace SkyJukebox.NAudioFramework
{
    public sealed class NAudioPlayer : IAudioPlayer
    {
        static NAudioPlayer()
        {
            var epath = Assembly.GetExecutingAssembly().Location;
            var exePath = epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
            // Load built-in NAudio codecs
            AddCodec(new string[] { "mp3", "wav", "m4a", "aac", "aiff", "mpc", "ape" }, typeof(AudioFileReader));
            AddCodec(new string[] { "wma" }, typeof(WMAFileReader));
            AddCodec(new string[] { "ogg" }, typeof(VorbisWaveReader));

            // Load external NAudio codecs
            foreach (var c in PluginInteraction.GetPlugins<ICodec>(exePath))
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
            _myAudioFileReader = new WaveChannel32(afr);
            _myWaveOut.Init(_myAudioFileReader);
            _myWaveOut.PlaybackStopped += MyWaveOutOnPlaybackStopped;
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

        public float Volume
        {
            get { return _myWaveOut != null ? _myAudioFileReader.Volume : 1; }
            set { if (_myWaveOut != null) _myAudioFileReader.Volume = value; }
        }

        public float Balance
        {
            get { return _myAudioFileReader.Pan; }
            set { _myAudioFileReader.Pan = value; }
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
    }
}
