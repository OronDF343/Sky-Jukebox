using System.IO;
using File = TagLib.File;

namespace SkyJukebox
{
    public class Music
    {
        public Music(string filePath)
        {
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
            get
            {
                return _path.SubstringRange(_path.LastIndexOf('\\') + 1, _path.LastIndexOf('.'));
            }
        }
        private string _ext;
        public string Extension
        {
            get
            {
                if (_ext == null)
                {
                    FileInfo f = GetFileInfo();
                    if (!f.Exists)
                        throw new FileNotFoundException("File not found: " + f);
                    _ext = f.Extension.ToLower().TrimStart('.');
                }
                return _ext;
            }
        }

        public File TagFile { get; private set; }

        public FileInfo GetFileInfo()
        {
            return new FileInfo(FilePath);
        }

        private void Initialize(string filePath)
        {
            _path = filePath;
            TagFile = File.Create(_path);
            //System.Windows.Forms.MessageBox.Show(TagFile.Properties.Codecs.ToArray()[0].Description + " " + TagFile.Tag.Title);
        }
    }
}
