using System.Collections.Generic;

namespace SkyJukebox.CoreApi.Contracts
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
