using System.Windows;
using System.Windows.Media;

namespace SkyJukebox.Utils
{
    public static class MouseUtils
    {
        public static Point CorrectGetPosition(Visual relativeTo)
        {
            var w32Mouse = new NativeMethods.Win32Point();
            NativeMethods.GetCursorPos(ref w32Mouse);
            return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
        }
        public static Point CorrectGetPosition()
        {
            var w32Mouse = new NativeMethods.Win32Point();
            NativeMethods.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
    }
}
