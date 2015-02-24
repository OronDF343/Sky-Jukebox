using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ShellDll;

namespace SkyJukebox.Lib.Wpf.TreeBrowser
{
    public static class DirectoryUtils
    {
        public static string ParseNameEx(this DirectoryInfoEx dx)
        {
            if (dx.Name.StartsWith("::{") && (dx.Name.EndsWith("}")))
                return GetDisplayName(dx);
            if (dx.Name.EndsWith(".library-ms"))
                return dx.Name.Substring(0, dx.Name.Length - 11);
            return dx.Name;
        }

        private static bool? _isWin81;
        public static bool IsWindows81OrHigher
        {
            get { return (_isWin81 ?? (_isWin81 = Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 3)).Value; }
        }


        private static bool? _isWin8;
        public static bool IsWindows8OrHigher
        {
            get { return (_isWin8 ?? (_isWin8 = Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2)).Value; }
        }

        private static bool? _isWin7;
        public static bool IsWindows7OrHigher
        {
            get { return (_isWin7 ?? (_isWin7 = Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)).Value; }
        }

        public static string GetDisplayName(DirectoryInfoEx dx)
        {
            IntPtr pidl;
            uint pchEaten = 0;
            var sfgao = new ShellAPI.SFGAO();
            DirectoryInfoEx.DesktopDirectory.ShellFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, dx.FullName, ref pchEaten, out pidl, ref sfgao);

            var ptrStr = Marshal.AllocCoTaskMem(ShellAPI.MAX_PATH * 2 + 4);
            Marshal.WriteInt32(ptrStr, 0, 0);
            var buf = new StringBuilder(ShellAPI.MAX_PATH);
            try
            {
                DirectoryInfoEx.DesktopDirectory.ShellFolder.GetDisplayNameOf(pidl, ShellAPI.SHGNO.NORMAL, ptrStr);
                ShellAPI.StrRetToBuf(ptrStr, pidl, buf, ShellAPI.MAX_PATH);
            }
            finally
            {
                if (ptrStr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptrStr);
                ptrStr = IntPtr.Zero;
            }

            return buf.ToString();
        }
    }
}
