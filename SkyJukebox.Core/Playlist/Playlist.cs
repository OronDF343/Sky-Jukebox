using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Lib.Collections;

namespace SkyJukebox.Core.Playlist
{
    public class Playlist : ObservableCollection<IMusicInfo>, IPlaylist
    {
        private int[] _shuffleMap;
        private readonly Random _randomizer = new Random();

        public void AddRange(string playlist, Action<Exception, string> errorCallback)
        {
            IEnumerable<string> e;
            var s = PlaylistDataManager.Instance.ReadPlaylist(playlist, out e);
            if (!s)
                errorCallback(new Exception("Failed to read the playlist"), playlist);
            else
                AddRange(e.Select(f => MusicInfo.Create(f, errorCallback)).Where(m => m != null));
        }

        public void AddRange(IEnumerable<IMusicInfo> items)
        {
            foreach (var i in items)
                Add(i);
        }

        /// <summary>
        /// Recreates the shuffle map.
        /// </summary>
        public void Reshuffle()
        {
            _shuffleMap = new int[Count];
            for (var i = 0; i < Count; ++i)
                _shuffleMap[i] = i;
            for (var i = 0; i < Count; ++i)
            {
                var r = _randomizer.Next(Count);
                var temp = _shuffleMap[i];
                _shuffleMap[i] = _shuffleMap[r];
                _shuffleMap[r] = temp;
            }
        }

        private bool _shuffle;
        /// <summary>
        /// Gets or sets a boolean value indicating whether the index will be shuffled.
        /// </summary>
        public bool ShuffleIndex
        {
            get { return _shuffle; }
            set
            {
                _shuffle = value;
                if (value && Count > 0)
                    Reshuffle();
            }
        }

        public void Sort(Comparison<IMusicInfo> comparison)
        {
            ArrayList.Adapter(this).Sort(new SortComparer<IMusicInfo>(comparison));
        }

        public int ShuffledIndexOf(IMusicInfo item)
        {
            var i = IndexOf(item);
            return ShuffleIndex ? (i > -1 ? _shuffleMap[i] : -1) : i;
        }

        public int ShuffledIndexOf(int index)
        {
            return ShuffleIndex && index > -1 && index < Count ? _shuffleMap[index] : index;
        }

        public int AbsoluteIndexOf(int index)
        {
            return ShuffleIndex && index > -1 && index < Count ? Array.IndexOf(_shuffleMap, index) : index;
        }

        public new IMusicInfo this[int index]
        {
            get
            {
                return ShuffleIndex ? base[_shuffleMap[index]] : base[index];
            }
            set
            {
                if (ShuffleIndex)
                    base[_shuffleMap[index]] = value;
                else
                    base[index] = value;
            }
        }
    }
}
