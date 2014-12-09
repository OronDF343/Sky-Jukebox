using System;
using System.IO;

namespace SkyJukebox.Api
{
    public interface IMusicInfo
    {
        string FilePath { get; }
        string FileName { get; }
        string Extension { get; }
        TimeSpan Duration { get; }
        int Bitrate { get; }
        TagLib.Tag Tag { get; }
    }
}
