using System;
using System.Runtime.InteropServices;

namespace SkyJukebox
{
    static class NativeMethods
    {
        internal static IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint RegisterWindowMessage(string message);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
    }
}
