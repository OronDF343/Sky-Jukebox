using System.Drawing;
using SkyJukebox.CoreApi.Contracts;

namespace SkyJukebox.CoreApi.Icons
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
