using System.Collections.Generic;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public class WmaCodec : ICodec
    {
        public string Name { get { return typeof(WMAFileReader).FullName; } }
        public WaveStream CreateWaveStream(string path)
        {
            return new WMAFileReader(path);
        }

        public IEnumerable<string> Extensions { get { return new[] { "wma" }; } }
    }
}
