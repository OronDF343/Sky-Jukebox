using System.Collections.Generic;
using NAudio.Wave;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public class CoreCodec : ICodec
    {
        public string Name { get { return typeof(AudioFileReader).FullName; } }
        public WaveStream CreateWaveStream(string path)
        {
            return new AudioFileReader(path);
        }

        public IEnumerable<string> Extensions { get { return new[] { "mp3", "wav", "m4a", "aac", "aiff", "ape", "wma" }; } }
    }
}
