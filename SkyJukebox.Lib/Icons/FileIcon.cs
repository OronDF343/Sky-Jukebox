using System.Drawing;

namespace SkyJukebox.Lib.Icons
{
    public class FileIcon : IconBase
    {
        public FileIcon(string path)
        {
            Path = path;
        }

        public override Image Image
        {
            get { return InnerImage ?? (InnerImage = new Bitmap(Path)); }
        }
    }
}
