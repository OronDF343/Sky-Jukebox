﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkyJukebox.Api.Playback
{
    public interface IAudioPlayer : IDisposable
    {
        /// <summary>
        /// The unique ID of the extension to which this object belongs.
        /// </summary>
        string ExtensionId { get; }
        /// <summary>
        /// An event which must be fired only when the end of the audio file is reached and playback stops.
        /// </summary>
        event EventHandler PlaybackFinished;
        /// <summary>
        /// An event which must be fired only if there is an error during playback (anywhere except for the Load function).
        /// </summary>
        event EventHandler PlaybackError;
        /// <summary>
        /// Prepare to play an audio file.
        /// </summary>
        /// <param name="path">The path to the audio file</param>
        /// <param name="device">The Guid of the output device</param>
        /// <returns>A boolean value indicating whether loading was successful</returns>
        bool Load(string path, Guid device);
        /// <summary>
        /// Unloads the currently loaded file. this includes stopping playback if necessary.
        /// </summary>
        void Unload();
        /// <summary>
        /// Begin playback of the loaded file from the beginning of the file.
        /// </summary>
        void Play();
        /// <summary>
        /// Pause playback of the current file. Will be called only if currently playing.
        /// </summary>
        void Pause();
        /// <summary>
        /// Resume playback of the current file. Will be called only if currently paused.
        /// </summary>
        void Resume();
        /// <summary>
        /// Stops playback of the current file.
        /// </summary>
        void Stop();
        /// <summary>
        /// Gets or sets the playback volume. Default is 1.0.
        /// </summary>
        decimal Volume { get; set; }
        /// <summary>
        /// Gets or sets the right/left volume balance. -1.0 to 1.0.
        /// </summary>
        decimal Balance { get; set; }
        /// <summary>
        /// Gets the duration of the currently loaded file.
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Gets or sets the current position in the loaded file.
        /// </summary>
        TimeSpan Position { get; set; }
        /// <summary>
        /// Gets the supported file extensions.
        /// </summary>
        IEnumerable<string> Extensions { get; }
        /// <summary>
        /// Gets a boolean value indicating whether a file is currently loaded.
        /// </summary>
        bool IsSomethingLoaded { get; }
        /// <summary>
        /// Gets or sets whether to enable EQ
        /// </summary>
        bool EnableEqualizer { get; set; }
        /// <summary>
        /// Gets a collection of the bands used in the EQ
        /// </summary>
        ObservableCollection<IEqualizerBand> EqualizerBands { get; }
    }
}
