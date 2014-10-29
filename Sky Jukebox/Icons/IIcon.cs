using System.Windows.Media;
using System.Drawing;
using Color = System.Drawing.Color;

namespace SkyJukebox.Icons
{
    public interface IIcon
    {
        string Path { get; }
        Image GetImage();
        ImageSource GetImageSource();
        bool IsRecolored { get; }
        void SetRecolor(Color c);
        void ResetColor();
    }
}
