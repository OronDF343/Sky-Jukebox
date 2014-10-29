using System;
using System.Collections.Generic;
using SkyJukebox.CoreApi.Contracts;

namespace NAudioFlacBox
{
    public class FlacCodec : ICodec
    {
        public Type WaveStreamType { get { return typeof(FlacFileReader); } }
        public IEnumerable<string> Extensions { get { return new string[] { "flac" }; } }
    }
}
