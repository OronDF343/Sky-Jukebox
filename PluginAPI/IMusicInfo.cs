using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.PluginAPI
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
