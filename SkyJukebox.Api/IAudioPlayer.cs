using System;
using System.Collections.Generic;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Api
{
    [ExtensionContract("AudioPlayer", "1.0.0.0")]
    public interface IAudioPlayer : IDisposable
    {
        /// <summary>
        /// An event which is fired only when the end of the audio file is reached and playback stops.
        /// </summary>
        event EventHandler PlaybackFinished;
        /// <summary>
        /// An event which is fired only if there is an error during playback (anywhere except for the Load function).
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
        /// Gets or sets the playback volume.
        /// </summary>
        float Volume { get; set; }
        /// <summary>
        /// Gets or sets the right/left volume balance.
        /// </summary>
        float Balance { get; set; }
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
        /// Gets the duration of a file which will not be played yet.
        /// </summary>
        /// <param name="file">The file path</param>
        /// <returns>The duration</returns>
        TimeSpan GetDuration(string file);
        /// <summary>
        /// Gets the length of a file which will not be played yet.
        /// </summary>
        /// <param name="file">The file path</param>
        /// <returns>The length</returns>
        long GetLength(string file);
    }
}
