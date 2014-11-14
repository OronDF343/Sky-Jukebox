using System;
using System.IO;

namespace SkyJukebox.Api
{
    public interface IMusicInfo
    {
        string FilePath { get; set; }
        string FileName { get; }
        string Extension { get; }
        FileInfo MusicFileInfo { get; }
        TimeSpan Duration { get; }
        int Bitrate { get; }
        TagLib.File TagFile { get; }
    }
}
