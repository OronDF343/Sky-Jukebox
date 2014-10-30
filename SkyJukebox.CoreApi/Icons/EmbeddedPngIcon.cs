using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using SkyJukebox.CoreApi.Contracts;

namespace SkyJukebox.CoreApi.Icons
{
    public class EmbeddedPngIcon : IconBase
    {
        public EmbeddedPngIcon(string path)
        {
            Path = path;
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
