using System;
using System.Collections.Generic;

namespace SkyJukebox.Api
{
    public interface IPlaybackManager
    {
        void Previous();
        void PlayPauseResume();
        void Next();
        void Stop();
        void RegisterAudioPlayer(IEnumerable<string> extensions, IAudioPlayer player);
        bool HasSupportingPlayer(string extension);
        Dictionary<string, IEnumerable<string>> GetAudioPlayerInfo();
        IPlaylist Playlist { get; }
        int NowPlayingId { get; set; }
        IMusicInfo NowPlaying { get; }
        float Volume { get; set; }
        float Balance { get; set; }
        TimeSpan Position { get; set; }
        TimeSpan Duration { get; }
        bool Shuffle { get; set; }
        bool AutoPlay { get; set; }
        LoopTypes LoopType { get; set; }
    }
    public enum LoopTypes { None, Single, All }
}
