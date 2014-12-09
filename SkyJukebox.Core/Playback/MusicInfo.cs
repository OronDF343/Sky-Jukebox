using System;
using System.IO;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using TagLib;
using File = TagLib.File;

namespace SkyJukebox.Core.Playback
{
    public class MusicInfo : IMusicInfo
    {
        public MusicInfo(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);
            FilePath = filePath;
            var tagFile = File.Create(FilePath);
            Tag = tagFile.Tag;
            tagFile.Dispose();
        }

        public string FilePath { get; private set; }
        public string FileName
        {
            get { return FilePath.SubstringRange(FilePath.LastIndexOf('\\') + 1, FilePath.LastIndexOf('.')); }
        }
        private string _ext;
        public string Extension
        {
            get { return _ext ?? (_ext = FilePath.GetExt()); }
        }

        private TimeSpan? _duration;
        public TimeSpan Duration
        {
            get
            {
                return _duration == null ? (_duration = new TimeSpan(0, 0, (int)(PlaybackManager.Instance.GetDuration(FilePath).TotalSeconds))).Value : _duration.Value;
            }
        }

        private int? _bitrate;
        public int Bitrate
        {
            get
            {
                return _bitrate == null
                           ? (_bitrate = (int)(PlaybackManager.Instance.GetLength(FilePath) / Duration.TotalSeconds / 128)).Value
                           : _bitrate.Value;
            }
        }

        public Tag Tag { get; private set; }
    }
}
