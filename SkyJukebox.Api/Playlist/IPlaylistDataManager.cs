using System;
using System.Collections.Generic;

namespace SkyJukebox.Api.Playlist
{
    public interface IPlaylistDataManager
    {
        bool ReadPlaylist(string path, out IEnumerable<string> entries);
        bool WritePlaylist(string path, IEnumerable<string> entries);

        void AddReader(IPlaylistReader reader);
        bool RemoveReader(string id);
        IPlaylistReader GetReader(string id);
        IPlaylistReader GetReaderByExtension(string ext);
        bool HasReader(string ext);
        IEnumerable<IPlaylistReader> GetReadersByExtension(string ext);
        IPlaylistReader FirstReader(Func<IPlaylistReader, bool> match);
        IPlaylistReader FirstReaderOrDefault(Func<IPlaylistReader, bool> match);
        IEnumerable<IPlaylistReader> GetAllReaders();
        IEnumerable<IPlaylistReader> FindAllReaders(Func<IPlaylistReader, bool> match);

        void AddWriter(IPlaylistWriter writer);
        bool RemoveWriter(string id);
        IPlaylistWriter GetWriter(string id);
        IPlaylistWriter GetWriterByExtension(string ext);
        bool HasWriter(string ext);
        IEnumerable<IPlaylistWriter> GetWritersByExtension(string ext);
        IPlaylistWriter FirstWriter(Func<IPlaylistWriter, bool> match);
        IPlaylistWriter FirstWriterOrDefault(Func<IPlaylistWriter, bool> match);
        IEnumerable<IPlaylistWriter> GetAllWriters();
        IEnumerable<IPlaylistWriter> FindAllWriters(Func<IPlaylistWriter, bool> match);
    }
}
