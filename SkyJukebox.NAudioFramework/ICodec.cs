using System;
using System.Collections.Generic;

namespace SkyJukebox.NAudioFramework
{
    public interface ICodec
    {
        Type WaveStreamType { get; }
        IEnumerable<string> Extensions { get; }
    }
}
