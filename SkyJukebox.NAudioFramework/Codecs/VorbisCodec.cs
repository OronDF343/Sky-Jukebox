using System.Collections.Generic;
using NAudio.Vorbis;
using NAudio.Wave;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public class VorbisCodec : ICodec
    {
        public string Name { get { return typeof(VorbisWaveReader).FullName; }}
        public WaveStream CreateWaveStream(string path)
        {
            return new VorbisWaveReader(path);
        }

        public IEnumerable<string> Extensions { get { return new[] { "ogg" }; } }
    }
}
