using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace SkyJukebox.Icons
{
    public abstract class Icon
    {
        protected Icon() { }
        public Icon(string path)
        {
            Path = path;
        }
        public string Path { get; private set; }
        public abstract Image GetImage();
        public abstract ImageSource GetImageSource();
        public bool IsRecolored { get; set; }
        public abstract void SetRecolor(Color c);
        public abstract void ResetColor();
    }
}
