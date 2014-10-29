using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using SkyJukebox.PluginAPI;
using SkyJukebox.Utils;

namespace SkyJukebox.Playback
{
    public sealed class NAudioPlayer : IAudioPlayer
    {
        private static readonly Dictionary<IEnumerable<string>, Type> Codecs = new Dictionary<IEnumerable<string>, Type>();
        public static void AddCodec(IEnumerable<string> exts, Type t)
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
        private WaveStream _myAudioFileReader;

        public event EventHandler PlaybackFinished;

        public event EventHandler PlaybackError;

        public bool Load(string path, Guid device)
        {
            var cext = path.GetExt();
            _myWaveOut = new DirectSoundOut(device);
            try
            {
                _myAudioFileReader = Activator.CreateInstance((from c in Codecs
                                                               where c.Key.Contains(cext)
                                                               select c.Value).First(), path) as WaveStream;
            }
            catch
            {
                return false;
            }
            if (_myAudioFileReader == null) return false;
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
            get { return _myWaveOut != null ? _myWaveOut.Volume : 1; }
            set { if (_myWaveOut != null) _myWaveOut.Volume = value; }
        }

        public float Balance
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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


        public static IEnumerable<string> GetCodecs()
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
