using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using SkyJukebox.PluginAPI;

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
            _myWaveOut.PlaybackStopped += MyWaveOutOnPlaybackStopped;
            _myWaveOut.Init(_myAudioFileReader);
            return true;
        }

        private void MyWaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (PlaybackFinished != null) PlaybackFinished(this, new EventArgs());
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
            _myWaveOut.Play();
        }

        public void Pause()
        {
            _myWaveOut.Pause();
        }

        public void Resume()
        {
            _myWaveOut.Play();
        }

        public void Stop()
        {
            if (_myWaveOut != null)
                _myWaveOut.Stop();
        }

        public float Volume
        {
            get { return _myWaveOut != null ? _myWaveOut.Volume : 1; }
            set { if (_myWaveOut != null) _myWaveOut.Volume = value; }
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
