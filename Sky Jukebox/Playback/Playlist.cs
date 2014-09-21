using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyJukebox
{
    public class Playlist : List<Music>
    {
        private int[] _shuffleMap;
        private readonly Random _randomizer = new Random();
        public Playlist()
        { }
        public Playlist(IEnumerable<Music> list) : base(list) { }
        public Playlist(string fileName)
        {
            AddRange(fileName);
        }
        public void Add(string fileName)
        {
            Add(new Music(fileName));
        }

        public void AddRange(string playlist)
        {
            var dir = new FileInfo(playlist).DirectoryName;
            AddRange(from f in File.ReadAllLines(playlist)
                     where f.Substring(0, 4) != "#EXT" && f != ""
                     select new Music(f[1] == ':' ? f : (dir + "\\" + f)));
        }
        public void AddRange(string folderName, bool subfolders)
        {
            if (subfolders)
                AddRange(from f in Util.GetFiles(folderName)
                         select new Music(f));
            else
                AddRange(from f in new DirectoryInfo(folderName).GetFiles()
                         select new Music(f.FullName));
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
        public new Music this[int index]
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
