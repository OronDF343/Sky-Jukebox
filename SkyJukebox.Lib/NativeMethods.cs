using System;
using System.Runtime.InteropServices;

namespace SkyJukebox.Lib
{
    /// <summary>
    /// FxCop requires all Marshalled functions to be in a class called NativeMethods.
    /// </summary>
    public static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        internal static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

#if WIN32
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        internal static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);
#else
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif

#if WIN32
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        internal static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);
#else
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#endif


        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableBlurBehindWindow(
            IntPtr hWnd, DwmBlurbehind pBlurBehind);

        [Flags]
        internal enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }
        [DllImport("dwmapi.dll", PreserveSig = true)]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd,
                                                       DwmWindowAttribute dwmAttribute,
                                                       ref int pvAttribute,
                                                       uint cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        internal class DwmBlurbehind
        {
            public uint dwFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fEnable;
            public IntPtr hRegionBlur;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fTransitionOnMaximized;

            // ReSharper disable InconsistentNaming
            public const uint DWM_BB_ENABLE = 0x00000001;
            public const uint DWM_BB_BLURREGION = 0x00000002;
            public const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;
            // ReSharper restore InconsistentNaming
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        public enum SHSTOCKICONID : uint
        {
            SIID_DOCNOASSOC = 0,
            SIID_DOCASSOC = 1,
            SIID_APPLICATION = 2,
            SIID_FOLDER = 3,
            SIID_FOLDEROPEN = 4,
            SIID_DRIVE525 = 5,
            SIID_DRIVE35 = 6,
            SIID_DRIVEREMOVE = 7,
            SIID_DRIVEFIXED = 8,
            SIID_DRIVENET = 9,
            SIID_DRIVENETDISABLED = 10,
            SIID_DRIVECD = 11,
            SIID_DRIVERAM = 12,
            SIID_WORLD = 13,
            SIID_SERVER = 15,
            SIID_PRINTER = 16,
            SIID_MYNETWORK = 17,
            SIID_FIND = 22,
            SIID_HELP = 23,
            SIID_SHARE = 28,
            SIID_LINK = 29,
            SIID_SLOWFILE = 30,
            SIID_RECYCLER = 31,
            SIID_RECYCLERFULL = 32,
            SIID_MEDIACDAUDIO = 40,
            SIID_LOCK = 47,
            SIID_AUTOLIST = 49,
            SIID_PRINTERNET = 50,
            SIID_SERVERSHARE = 51,
            SIID_PRINTERFAX = 52,
            SIID_PRINTERFAXNET = 53,
            SIID_PRINTERFILE = 54,
            SIID_STACK = 55,
            SIID_MEDIASVCD = 56,
            SIID_STUFFEDFOLDER = 57,
            SIID_DRIVEUNKNOWN = 58,
            SIID_DRIVEDVD = 59,
            SIID_MEDIADVD = 60,
            SIID_MEDIADVDRAM = 61,
            SIID_MEDIADVDRW = 62,
            SIID_MEDIADVDR = 63,
            SIID_MEDIADVDROM = 64,
            SIID_MEDIACDAUDIOPLUS = 65,
            SIID_MEDIACDRW = 66,
            SIID_MEDIACDR = 67,
            SIID_MEDIACDBURN = 68,
            SIID_MEDIABLANKCD = 69,
            SIID_MEDIACDROM = 70,
            SIID_AUDIOFILES = 71,
            SIID_IMAGEFILES = 72,
            SIID_VIDEOFILES = 73,
            SIID_MIXEDFILES = 74,
            SIID_FOLDERBACK = 75,
            SIID_FOLDERFRONT = 76,
            SIID_SHIELD = 77,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80,
            SIID_KEY = 81,
            SIID_SOFTWARE = 82,
            SIID_RENAME = 83,
            SIID_DELETE = 84,
            SIID_MEDIAAUDIODVD = 85,
            SIID_MEDIAMOVIEDVD = 86,
            SIID_MEDIAENHANCEDCD = 87,
            SIID_MEDIAENHANCEDDVD = 88,
            SIID_MEDIAHDDVD = 89,
            SIID_MEDIABLURAY = 90,
            SIID_MEDIAVCD = 91,
            SIID_MEDIADVDPLUSR = 92,
            SIID_MEDIADVDPLUSRW = 93,
            SIID_DESKTOPPC = 94,
            SIID_MOBILEPC = 95,
            SIID_USERS = 96,
            SIID_MEDIASMARTMEDIA = 97,
            SIID_MEDIACOMPACTFLASH = 98,
            SIID_DEVICECELLPHONE = 99,
            SIID_DEVICECAMERA = 100,
            SIID_DEVICEVIDEOCAMERA = 101,
            SIID_DEVICEAUDIOPLAYER = 102,
            SIID_NETWORKCONNECT = 103,
            SIID_INTERNET = 104,
            SIID_ZIPFILE = 105,
            SIID_SETTINGS = 106,
            SIID_DRIVEHDDVD = 132,
            SIID_DRIVEBD = 133,
            SIID_MEDIAHDDVDROM = 134,
            SIID_MEDIAHDDVDR = 135,
            SIID_MEDIAHDDVDRAM = 136,
            SIID_MEDIABDROM = 137,
            SIID_MEDIABDR = 138,
            SIID_MEDIABDRE = 139,
            SIID_CLUSTEREDDRIVE = 140,
            SIID_MAX_ICONS = 175
        }

        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        private const int MAX_PATH = 260;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPath;
        }

        #region SHGFI

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_PIDL = 0x000000008;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        public const uint SHGFI_SYSICONINDEX = 16384;
        public const uint SHGFI_USEFILEATTRIBUTES = 16;

        /// <summary>
        /// Get Icons that are associated with files.
        /// To use it, use (System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon));
        /// hImgSmall = SHGetFileInfo(fName, 0, ref shinfo,(uint)Marshal.SizeOf(shinfo),Win32.SHGFI_ICON |Win32.SHGFI_SMALLICON);
        /// </summary>
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes,
                                                  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
                                                  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        #endregion

        #region SystemImageList

        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;
        [DllImport("kernel32")]
        public extern static int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            string lpBuffer,
            uint nSize,
            int argumentsLong);

        [DllImport("kernel32")]
        public extern static int GetLastError();

        [DllImport("comctl32")]
        public extern static int ImageList_Draw(
            IntPtr hIml,
            int i,
            IntPtr hdcDst,
            int x,
            int y,
            int fStyle);

        [DllImport("comctl32")]
        public extern static int ImageList_DrawIndirect(
            ref IMAGELISTDRAWPARAMS pimldp);

        [DllImport("comctl32")]
        public extern static int ImageList_GetIconSize(
            IntPtr himl,
            ref int cx,
            ref int cy);

        [DllImport("comctl32")]
        public extern static int ImageList_GetImageInfo(
            IntPtr himl,
            int i,
            ref IMAGEINFO pImageInfo);

        [DllImport("comctl32")]
        public extern static IntPtr ImageList_GetIcon(
            IntPtr himl,
            int i,
            int flags);

        /// <summary>
        /// SHGetImageList is not exported correctly in XP.  See KB316931
        /// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
        /// Apparently (and hopefully) ordinal 727 isn't going to change.
        /// </summary>
        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(
            int iImageList,
            ref Guid riid,
            ref IImageList ppv
            );

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageListHandle(
            int iImageList,
            ref Guid riid,
            ref IntPtr handle
            );

        [Flags]
        public enum SHGetFileInfoConstants : int
        {
            SHGFI_ICON = 0x100,                // get icon 
            SHGFI_DISPLAYNAME = 0x200,         // get display name 
            SHGFI_TYPENAME = 0x400,            // get type name 
            SHGFI_ATTRIBUTES = 0x800,          // get attributes 
            SHGFI_ICONLOCATION = 0x1000,       // get icon location 
            SHGFI_EXETYPE = 0x2000,            // return exe type 
            SHGFI_SYSICONINDEX = 0x4000,       // get system icon index             
            SHGFI_LINKOVERLAY = 0x8000,        // put a link overlay on icon 
            SHGFI_SELECTED = 0x10000,          // show icon in selected state 
            SHGFI_ATTR_SPECIFIED = 0x20000,    // get only specified attributes 
            SHGFI_LARGEICON = 0x0,             // get large icon 
            SHGFI_SMALLICON = 0x1,             // get small icon 
            SHGFI_OPENICON = 0x2,              // get open icon 
            SHGFI_SHELLICONSIZE = 0x4,         // get shell size icon 
            SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
            SHGFI_USEFILEATTRIBUTES = 0x10,     // use passed dwFileAttribute 
            SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040     // Get the index of the overlay
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;        // x offest from the upperleft of bitmap
            public int yBitmap;        // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [ComImport]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //helpstring("Image List"),
        public interface IImageList
        {
            [PreserveSig]
            int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

            [PreserveSig]
            int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

            [PreserveSig]
            int SetOverlayImage(
                int iImage,
                int iOverlay);

            [PreserveSig]
            int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

            [PreserveSig]
            int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
                int i);

            [PreserveSig]
            int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(
                int i,
                ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(
                int iDst,
                IImageList punkSrc,
                int iSrc,
                int uFlags);

            [PreserveSig]
            int Merge(
                int i1,
                IImageList punk2,
                int i2,
                int dx,
                int dy,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int Clone(
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(
                int i,
                ref RECT prc);

            [PreserveSig]
            int GetIconSize(
                ref int cx,
                ref int cy);

            [PreserveSig]
            int SetIconSize(
                int cx,
                int cy);

            [PreserveSig]
            int GetImageCount(
                ref int pi);

            [PreserveSig]
            int SetImageCount(
                int uNewCount);

            [PreserveSig]
            int SetBkColor(
                int clrBk,
                ref int pclr);

            [PreserveSig]
            int GetBkColor(
                ref int pclr);

            [PreserveSig]
            int BeginDrag(
                int iTrack,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(
                IntPtr hwndLock,
                int x,
                int y);

            [PreserveSig]
            int DragLeave(
                IntPtr hwndLock);

            [PreserveSig]
            int DragMove(
                int x,
                int y);

            [PreserveSig]
            int SetDragCursorImage(
                ref IImageList punk,
                int iDrag,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int DragShowNolock(
                int fShow);

            [PreserveSig]
            int GetDragImage(
                ref POINT ppt,
                ref POINT pptHotspot,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(
                int i,
                ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(
                int iOverlay,
                ref int piIndex);
        };
        #endregion
    }
}
