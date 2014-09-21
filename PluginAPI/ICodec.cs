using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.PluginAPI
{
    public interface ICodec
    {
        Type WaveStreamType { get; }
        IEnumerable<string> Extensions { get; }
    }
}
