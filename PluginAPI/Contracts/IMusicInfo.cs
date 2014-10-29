using System.IO;

namespace SkyJukebox.CoreApi.Contracts
{
    public interface IMusicInfo
    {
        string FilePath { get; set; }
        string FileName { get; }
        string Extension { get; }
        FileInfo MusicFileInfo { get; }
        TagLib.File TagFile { get; }
    }
}
