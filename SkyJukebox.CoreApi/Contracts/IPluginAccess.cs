using System;

namespace SkyJukebox.CoreApi.Contracts
{
    /// <summary>
    /// Provides access to functions of Sky Jukebox.
    /// An object of this type is passed to the Plugin on initialization.
    /// </summary>
    public interface IPluginAccess
    {
        IPlaybackManager GetPlaybackManager();
        IconBase CreateFileIcon(string path);
        IconBase CreateEmbeddedIcon(string path);
        // TODO: Implement stuff
    }
}
