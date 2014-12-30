using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace SkyJukebox.Lib.Icons
{
    public interface IIcon
    {
        string Path { get; }
        Image Image { get; }

        ImageSource ImageSource { get; }
        bool IsRecolored { get; }
        void SetRecolor(Color c);
        void ResetColor();
    }
}
