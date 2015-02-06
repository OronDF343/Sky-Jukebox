using System;
using System.Collections.Generic;
using System.ComponentModel;
using SkyJukebox.Api.Playlist;

namespace SkyJukebox.Api.Playback
{
    public interface IPlaybackManager : INotifyPropertyChanged
    {
        void Previous();
        void PlayPauseResume();
        void Next();
        void Stop();
        void Reset();
        void RegisterAudioPlayer(IEnumerable<string> extensions, IAudioPlayer player);
        bool HasSupportingPlayer(string extension);
        Dictionary<string, IEnumerable<string>> GetAudioPlayerInfo();
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        ICollection<string> SupportedFileTypes { get; }
        IPlaylist Playlist { get; }
        // ReSharper restore ReturnTypeCanBeEnumerable.Global
        int NowPlayingId { get; set; }
        int AbsoluteNowPlayingId { get; set; }
        IMusicInfo NowPlaying { get; }
        decimal Volume { get; set; }
        decimal Balance { get; set; }
        TimeSpan Position { get; set; }
        TimeSpan Duration { get; }
        bool Shuffle { get; set; }
        bool AutoPlay { get; set; }
        LoopTypes LoopType { get; set; }
    }
    public enum LoopTypes { None, Single, All }
}
