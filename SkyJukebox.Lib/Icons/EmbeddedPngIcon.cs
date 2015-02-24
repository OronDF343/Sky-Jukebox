using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SkyJukebox.Lib.Icons
{
    public class EmbeddedPngIcon : IconBase
    {
        public EmbeddedPngIcon(string path, bool allowRecolor = true)
        {
            Path = path;
            AllowRecolor = allowRecolor;
        }

        public override Image Image
        {
            get
            {
                if (InnerImage != null) return InnerImage;
                var ms = new MemoryStream();
                var bbe = new PngBitmapEncoder();
                bbe.Frames.Add(BitmapFrame.Create(new Uri(Path)));
                bbe.Save(ms);
                return InnerImage = Image.FromStream(ms);
            }
        }
    }
}
