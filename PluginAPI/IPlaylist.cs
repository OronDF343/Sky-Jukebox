using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.PluginAPI
{
    public interface IPlaylist : IList<IMusicInfo>
    {
        void Add(string fileName);
        void AddRange(string playlist);
        void AddRange(string folderName, bool subfolders);
        void AddRange(IEnumerable<IMusicInfo> items);
        void Reshuffle();
        bool ShuffleIndex { get; set; }
    }
}
