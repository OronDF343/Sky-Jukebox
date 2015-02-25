using System.Collections.Generic;
using NAudio.Flac;
using NAudio.Wave;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public class FlacCodec : ICodec
    {
        public string Name { get { return typeof(FlacReader).FullName; } }
        public WaveStream CreateWaveStream(string path)
        {
            return new FlacReader(path);
        }

        public IEnumerable<string> Extensions { get { return new[] { "flac" }; } }
    }
}
