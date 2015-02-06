using System.Collections.Generic;

namespace SkyJukebox.Api.Playlist
{
    /// <summary>
    /// Provides methods used to read a specific kind of playlist file.
    /// </summary>
    public interface IPlaylistReader
    {
        /// <summary>
        /// Gets the internal ID of the <see cref="IPlaylistReader"/>.
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
        /// Reads file entries from a playlist file.
        /// </summary>
        /// <param name="path">The path to the playlist file.</param>
        /// <param name="entries">A <see cref="T:IEnumerable`1{string}"/> which will contain the entries from the playlist file.</param>
        /// <returns>A boolean value indicating whether the operation completed successfully.</returns>
        bool GetPlaylistFiles(string path, out IEnumerable<string> entries);
    }
}
