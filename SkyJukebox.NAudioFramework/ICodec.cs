using System;
using System.Collections.Generic;
using SkyJukebox.Api;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.NAudioFramework
{
    [ExtensionContract("Codec", "1.0.0.0")]
    public interface ICodec
    {
        Type WaveStreamType { get; }
        IEnumerable<string> Extensions { get; }
    }
}
