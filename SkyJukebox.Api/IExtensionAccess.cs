using System;
using SkyJukebox.Api.Playback;
using SkyJukebox.Api.Playlist;

namespace SkyJukebox.Api
{
    /// <summary>
    /// Provides access to the functions of Sky Jukebox.
    /// An object of this type is passed to every extension on initialization.
    /// </summary>
    public interface IExtensionAccess
    {
        // Get instances:
        /// <summary>
        /// Manages all the <see cref="IAudioPlayer"/> implementations, the current playlist, and controls all the playback logic.
        /// </summary>
        IPlaybackManager PlaybackManagerInstance { get; }
        /// <summary>
        /// Manages all the <see cref="SkyJukebox.Lib.Icons.IIcon"/> objects used in the GUIs. These can be modified by skins.
        /// </summary>
        IIconManager IconManagerInstance { get; }
        /// <summary>
        /// Manages all the settings of Sky Jukebox and manages loading/saving them from/to XML.
        /// </summary>
        /// <remarks>Each setting implements INotifyPropertyChanged.</remarks>
        ISettingsManager SettingsManagerInstance { get; }
        /// <summary>
        /// Manages <see cref="IPlaylistReader"/> and <see cref="IPlaylistWriter"/> implementations to provide a centralized way to read/write playlists.
        /// </summary>
        IPlaylistDataManager PlaylistDataManagerInstance { get; }
        /// <summary>
        /// Contains strings required for several different components.
        /// Examples: File paths, program id, command line arguments, release tag name.
        /// </summary>
        IInstanceManager InstanceManagerInstance { get; }

        // UI:
        /// <summary>
        /// Creates or modifies a button in the Plugins Widget. Set a value to null to prevent modifying it.
        /// InitGUI and later stages only!
        /// </summary>
        /// <param name="btnId">The unique ID to give the button. If the ID already exists, an existing button will be modified.</param>
        /// <param name="iconId">The ID of the icon to be used (from the IconManager).</param>
        /// <param name="onClick">An action to preform when the button is clicked. This value can't be changed after adding the button.</param>
        /// <param name="toolTip">The tooltip text for the button.</param>
        void AddPluginButton(string btnId, string iconId, Action onClick, string toolTip);
        /// <summary>
        /// Removes a button from the Plugins Widget.
        /// InitGUI and later stages only!
        /// </summary>
        /// <param name="btnId">The unique ID of the button</param>
        void RemovePluginButton(string btnId);
    }
}
