using System.Collections.Generic;

namespace SkyJukebox.Api.Playlist
{
    /// <summary>
    /// Provides methods used to write a specific kind of playlist file.
    /// </summary>
    public interface IPlaylistWriter
    {
        /// <summary>
        /// Internal ID of the <see cref="IPlaylistWriter"/>
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets the format name to be shown to the user.
        /// </summary>
        string FormatName { get; }
        /// <summary>
        /// Gets the possible file extensions for this format.
        /// </summary>
        IEnumerable<string> FormatExtensions { get; }
        /// <summary>
        /// Writes a list of entries to a playlist file.
        /// </summary>
        /// <param name="path">The path to the playlist file.</param>
        /// <param name="entries">The entries which will be written to the playlist file.</param>
        /// <returns>A boolean value indicating whether the operation completed successfully.</returns>
        bool WritePlaylist(string path, IEnumerable<string> entries);
    }
}
