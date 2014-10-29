using System.IO;
using SkyJukebox.Utils;
using File = TagLib.File;

namespace SkyJukebox.Playback
{
    public class Music
    {
        public Music(string filePath)
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
            //System.Windows.Forms.MessageBox.Show(TagFile.Properties.Codecs.ToArray()[0].Description + " " + TagFile.Tag.Title);
        }
    }
}
