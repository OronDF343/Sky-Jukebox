using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace SkyJukebox.Lib.Icons
{
    public abstract class IconBase : IIcon
    {
        public string Path { get; protected set; }
        protected Image InnerImage;
        public abstract Image Image { get; }
        protected ImageSource InnerImageSource;

        public ImageSource ImageSource
        {
            get { return InnerImageSource ?? (InnerImageSource = new BitmapImage(new Uri(Path))); }
        }
        public bool IsRecolored { get; protected set; }
        public void SetRecolor(Color c)
        {
            InnerImageSource = null;
            if (IsRecolored) InnerImage = null;
            IsRecolored = true;
            InnerImage = Image.RecolorFromGrayscale(c);
            InnerImageSource = InnerImage.ToBitmapSource();
        }
        public void ResetColor()
        {
            InnerImage = null;
            InnerImageSource = null;
            IsRecolored = false;
        }
    }
}
