using System;
using TagLib;

namespace SkyJukebox.Api.Playlist
{
    public interface IMusicInfo
    {
        Guid UniqueId { get; }
        string FilePath { get; }
        string FileName { get; }
        string Extension { get; }
        TimeSpan Duration { get; }
        int Bitrate { get; }
        Tag Tag { get; }
    }
}
