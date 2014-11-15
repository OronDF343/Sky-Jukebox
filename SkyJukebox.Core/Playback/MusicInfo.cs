using System;
using System.IO;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using File = TagLib.File;

namespace SkyJukebox.Core.Playback
{
    public class MusicInfo : IMusicInfo
    {
        public MusicInfo(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);
            Initialize(filePath);
        }
        private string _path;
        public string FilePath
        {
            get { return _path; }
            set { Initialize(value); }
        }
        public string FileName
        {
            get { return MusicFileInfo.Name.SubstringRange(0, MusicFileInfo.Name.LastIndexOf('.')); }
        }
        private string _ext;
        public string Extension
        {
            get { return _ext ?? (_ext = MusicFileInfo.Extension.ToLower().TrimStart('.')); }
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

        public File TagFile { get; private set; }

        private FileInfo _fileInfo;
        public FileInfo MusicFileInfo
        {
            get { return _fileInfo ?? (_fileInfo = new FileInfo(FilePath)); }
        }

        private void Initialize(string filePath)
        {
            _path = filePath;
            TagFile = File.Create(_path);
        }
    }
}
