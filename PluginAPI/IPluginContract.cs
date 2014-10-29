using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.PluginAPI
{
    /// <summary>
    /// Provides access to functions of Sky Jukebox.
    /// An object of this type is passed to the Plugin on initialization.
    /// </summary>
    public interface IPluginContract
    {
        IPlaybackManager GetPlaybackManager();
        // TODO: Implement stuff
    }
}
