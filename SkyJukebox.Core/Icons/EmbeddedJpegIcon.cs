using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SkyJukebox.Core.Icons
{
    public class EmbeddedJpegIcon : IconBase
    {
        public EmbeddedJpegIcon(string path)
        {
            Path = path;
        }

        public override Image Image
        {
            get
            {
                if (InnerImage != null) return InnerImage;
                var ms = new MemoryStream();
                var bbe = new JpegBitmapEncoder();
                bbe.Frames.Add(BitmapFrame.Create(new Uri(Path)));
                bbe.Save(ms);
                return InnerImage = Image.FromStream(ms);
            }
        }
    }
}
