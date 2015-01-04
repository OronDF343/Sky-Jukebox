﻿namespace SkyJukebox.Api
{
    /// <summary>
    /// Provides access to functions of Sky Jukebox.
    /// An object of this type is passed to the Plugin on initialization.
    /// </summary>
    public interface IPluginAccess
    {
        IIcon CreateFileIcon(string path);
        IIcon CreateEmbeddedIcon(string path);

        // Get instances:
        IPlaybackManager PlaybackManagerInstance { get; }
        ISettingsManager SettingsManagerInstance { get; }
    }
}
