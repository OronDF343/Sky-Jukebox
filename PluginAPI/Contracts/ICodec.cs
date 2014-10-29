using System;
using System.Collections.Generic;

namespace SkyJukebox.CoreApi.Contracts
{
    public interface ICodec
    {
        Type WaveStreamType { get; }
        IEnumerable<string> Extensions { get; }
    }
}
