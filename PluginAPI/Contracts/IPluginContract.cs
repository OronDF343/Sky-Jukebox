namespace SkyJukebox.CoreApi.Contracts
{
    /// <summary>
    /// Provides access to functions of Sky Jukebox.
    /// An object of this type is passed to the Plugin on initialization.
    /// </summary>
    public interface IPluginAccess
    {
        IPlaybackManager GetPlaybackManager();
        // TODO: Implement stuff
    }
}
