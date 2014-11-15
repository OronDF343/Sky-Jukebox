using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SkyJukebox.Api
{
    public interface IPlaylist : IList<IMusicInfo>, INotifyCollectionChanged
    {
        void Add(string fileName);
        void AddRange(string playlist);
        void AddRange(string folderName, bool subfolders);
        void AddRange(IEnumerable<IMusicInfo> items);
        void Reshuffle();
        bool ShuffleIndex { get; set; }
        void Move(int oldIndex, int newIndex);
        void Sort(Comparison<IMusicInfo> comparison);
    }
}
