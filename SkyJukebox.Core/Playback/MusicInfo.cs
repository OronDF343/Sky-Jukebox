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
            UniqueId = Guid.NewGuid();
            if (!FileEx.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);
            FilePath = filePath;
            GetInfoFromTag();
        }

        public MusicInfo(FileInfoEx file)
        {
            UniqueId = Guid.NewGuid();
            if (!file.Exists)
                throw new FileNotFoundException("File not found: " + file.FullName);
            FilePath = file.FullName;
            GetInfoFromTag();
        }

        public static MusicInfo Create(string filePath, Action<Exception, string> errorCallback)
        {
            try
            {
                return new MusicInfo(filePath);
            }
            catch (Exception ex)
            {
                errorCallback(ex, filePath);
                return null;
            }
        }

        public static MusicInfo Create(FileInfoEx file, Action<Exception, string> errorCallback)
        {
            try
            {
                return new MusicInfo(file);
            }
            catch (Exception ex)
            {
                errorCallback(ex, file.FullName);
                return null;
            }
        }

        public Guid UniqueId { get; private set; }
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

        public TimeSpan Duration { get; private set; }

        public int Bitrate { get; private set; }

        public Tag Tag { get; private set; }

        private void GetInfoFromTag()
        {
            var tagFile = File.Create(FilePath);
            Tag = tagFile.Tag;
            Duration = TimeSpan.FromSeconds((int)tagFile.Properties.Duration.TotalSeconds);
            Bitrate = tagFile.Properties.AudioBitrate;
            tagFile.Dispose();
        }

        public override bool Equals(object obj)
        {
            var info = obj as IMusicInfo;
            return info != null && UniqueId.Equals(info.UniqueId);
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }
    }
}
