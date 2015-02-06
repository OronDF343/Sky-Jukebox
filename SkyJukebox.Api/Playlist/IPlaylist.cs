using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SkyJukebox.Api.Playlist
{
    public interface IPlaylist : IList<IMusicInfo>, INotifyCollectionChanged
    {
        void AddRange(string playlist, Action<Exception, string> errorCallback);
        void AddRange(IEnumerable<IMusicInfo> items);
        void Reshuffle();
        bool ShuffleIndex { get; set; }
        void Move(int oldIndex, int newIndex);
        void Sort(Comparison<IMusicInfo> comparison);
        int ShuffledIndexOf(IMusicInfo item);
        int ShuffledIndexOf(int index);
        int AbsoluteIndexOf(int index);
    }
}
