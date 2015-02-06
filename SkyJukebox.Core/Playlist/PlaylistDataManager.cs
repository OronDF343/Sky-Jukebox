using System;
using System.Collections.Generic;
using System.Linq;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Lib;

namespace SkyJukebox.Core.Playlist
{
    public class PlaylistDataManager : IPlaylistDataManager
    {
        #region Singleton

        private PlaylistDataManager()
        {
            Readers = new Dictionary<string, IPlaylistReader>();
            Writers = new Dictionary<string, IPlaylistWriter>();
            // TODO: Put this somewhere else so the settings are added early on
            // ReSharper disable InconsistentNaming
            var m3ur = new M3UPlaylistReader();
            Readers.Add(m3ur.Id, m3ur);
            var m3uw = new M3UPlaylistWriter();
            Writers.Add(m3uw.Id, m3uw);
            // ReSharper restore InconsistentNaming
        }

        private static PlaylistDataManager _instance;
        public static PlaylistDataManager Instance { get { return _instance ?? (_instance = new PlaylistDataManager()); } }
        #endregion

        public bool ReadPlaylist(string path, out IEnumerable<string> entries)
        {
            try
            {
                return GetReaderByExtension(path.GetExt()).GetPlaylistFiles(path, out entries);
            }
            catch
            {
                entries = null;
                return false;
            }
        }

        public bool WritePlaylist(string path, IEnumerable<string> entries)
        {
            try
            {
                return GetWriterByExtension(path.GetExt()).WritePlaylist(path, entries);
            }
            catch
            {
                return false;
            }
        }

        internal readonly Dictionary<string, IPlaylistReader> Readers;
        internal readonly Dictionary<string, IPlaylistWriter> Writers;

        public void AddReader(IPlaylistReader reader)
        {
            Readers.Add(reader.Id, reader);
        }

        public bool RemoveReader(string id)
        {
            return Readers.Remove(id);
        }

        public IPlaylistReader GetReader(string id)
        {
            return Readers[id];
        }

        public IPlaylistReader GetReaderByExtension(string ext)
        {
            return Readers.Values.FirstOrDefault(r => r.FormatExtensions.Contains(ext));
        }

        public IEnumerable<IPlaylistReader> GetReadersByExtension(string ext)
        {
            return Readers.Values.Where(r => r.FormatExtensions.Contains(ext));
        }

        public bool HasReader(string ext)
        {
            return GetReaderByExtension(ext) != default(IPlaylistReader);
        }

        public IPlaylistReader FirstReader(Func<IPlaylistReader, bool> match)
        {
            return Readers.Values.First(match);
        }

        public IPlaylistReader FirstReaderOrDefault(Func<IPlaylistReader, bool> match)
        {
            return Readers.Values.FirstOrDefault(match);
        }

        public IEnumerable<IPlaylistReader> GetAllReaders()
        {
            return Readers.Values;
        }

        public IEnumerable<IPlaylistReader> FindAllReaders(Func<IPlaylistReader, bool> match)
        {
            return Readers.Values.Where(match);
        }



        public void AddWriter(IPlaylistWriter writer)
        {
            Writers.Add(writer.Id, writer);
        }

        public bool RemoveWriter(string id)
        {
            return Writers.Remove(id);
        }

        public IPlaylistWriter GetWriter(string id)
        {
            return Writers[id];
        }

        public IPlaylistWriter GetWriterByExtension(string ext)
        {
            return Writers.Values.FirstOrDefault(w => w.FormatExtensions.Contains(ext));
        }

        public IEnumerable<IPlaylistWriter> GetWritersByExtension(string ext)
        {
            return Writers.Values.Where(w => w.FormatExtensions.Contains(ext));
        }

        public bool HasWriter(string ext)
        {
            return GetWriterByExtension(ext) != default(IPlaylistWriter);
        }

        public IPlaylistWriter FirstWriter(Func<IPlaylistWriter, bool> match)
        {
            return Writers.Values.First(match);
        }

        public IPlaylistWriter FirstWriterOrDefault(Func<IPlaylistWriter, bool> match)
        {
            return Writers.Values.FirstOrDefault(match);
        }

        public IEnumerable<IPlaylistWriter> GetAllWriters()
        {
            return Writers.Values;
        }

        public IEnumerable<IPlaylistWriter> FindAllWriters(Func<IPlaylistWriter, bool> match)
        {
            return Writers.Values.Where(match);
        }
    }
}
