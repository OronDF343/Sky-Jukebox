using System.Drawing;
using System.Windows.Media;

namespace SkyJukebox.CoreApi.Contracts
{
    public interface IIcon
    {
        string Path { get; }
        Image GetImage();
        ImageSource GetImageSource();
        bool IsRecolored { get; }
        void SetRecolor(System.Drawing.Color c);
        void ResetColor();
    }
}
