using SkyJukebox.PluginAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudioFlacBox
{
    public class FlacCodec : ICodec
    {
        public Type WaveStreamType { get { return typeof(FlacFileReader); } }
        public IEnumerable<string> Extensions { get { return new string[] { "flac" }; } }
    }
}
