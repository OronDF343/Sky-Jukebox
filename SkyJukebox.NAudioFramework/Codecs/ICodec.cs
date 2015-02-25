using System.Collections.Generic;
using NAudio.Wave;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.NAudioFramework.Codecs
{
    [ExtensionContract("1.0.0.0", "1.0.0.0")]
    public interface ICodec
    {
        string Name { get; }
        WaveStream CreateWaveStream(string path);
        IEnumerable<string> Extensions { get; }
    }
}
