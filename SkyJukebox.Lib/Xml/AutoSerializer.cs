using System.IO;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public class AutoSerializer<T>
    {
        private readonly XmlSerializer _myXs = new XmlSerializer(typeof(T));

        public T LoadFromXml(string path)
        {
            try
            {
                var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var t = (T)_myXs.Deserialize(fs);
                fs.Close();
                return t;
            }
            catch
            {
                return default(T);
            }
        }
        public void SaveToXml(string path, T t)
        {
            if (path == null) return;
            if (!File.Exists(path))
            {
                // work around bug with File.Create()
                var cs = new FileStream(path, FileMode.Create, FileAccess.Write);
                cs.Close();
            }
            var fs = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            _myXs.Serialize(fs, t);
            fs.Close();
        }
    }
}
