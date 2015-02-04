// Created By LYCJ (2010), released under LGPL license
// Edited by Leung Yat Chun Joseph based on http://vbaccelerator.com/home/net/code/libraries/Shell_Projects/SysImageList/article.asp

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkyJukebox.Lib.Wpf.TreeBrowser
{
    public enum IconSize
    {
        Small = 0x1, Large = 0x0, ExtraLarge = 0x2, Jumbo = 0x4, Thumbnail = 0x5
    }

    #region Public Enumerations

    /// <summary>
    /// Flags controlling how the Image List item is 
    /// drawn
    /// </summary>
    [Flags]
    public enum ImageListDrawItemConstants
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Draw item normally.
        /// </summary>
        ILD_NORMAL = 0x0,
        /// <summary>
        /// Draw item transparently.
        /// </summary>
        ILD_TRANSPARENT = 0x1,
        /// <summary>
        /// Draw item blended with 25% of the specified foreground colour
        /// or the Highlight colour if no foreground colour specified.
        /// </summary>
        ILD_BLEND25 = 0x2,
        /// <summary>
        /// Draw item blended with 50% of the specified foreground colour
        /// or the Highlight colour if no foreground colour specified.
        /// </summary>
        ILD_SELECTED = 0x4,
        /// <summary>
        /// Draw the icon's mask
        /// </summary>
        ILD_MASK = 0x10,
        /// <summary>
        /// Draw the icon image without using the mask
        /// </summary>
        ILD_IMAGE = 0x20,
        /// <summary>
        /// Draw the icon using the ROP specified.
        /// </summary>
        ILD_ROP = 0x40,
        /// <summary>
        /// Preserves the alpha channel in dest. XP only.
        /// </summary>
        ILD_PRESERVEALPHA = 0x1000,
        /// <summary>
        /// Scale the image to cx, cy instead of clipping it.  XP only.
        /// </summary>
        ILD_SCALE = 0x2000,
        /// <summary>
        /// Scale the image to the current DPI of the display. XP only.
        /// </summary>
        ILD_DPISCALE = 0x4000,

        ILD_OVERLAYMASK = 0x00000F00,
        ILD_ASYNC = 0x00008000
    }

    /// <summary>
    /// Flags specifying the state of the icon to draw from the Shell
    /// </summary>
    [Flags]
    public enum ShellIconStateConstants
    {
        /// <summary>
        /// Get icon in normal state
        /// </summary>
        ShellIconStateNormal = 0,
        /// <summary>
        /// Put a link overlay on icon 
        /// </summary>
        ShellIconStateLinkOverlay = 0x8000,
        /// <summary>
        /// show icon in selected state 
        /// </summary>
        ShellIconStateSelected = 0x10000,
        /// <summary>
        /// get open icon 
        /// </summary>
        ShellIconStateOpen = 0x2,
        /// <summary>
        /// apply the appropriate overlays
        /// </summary>
        ShellIconAddOverlays = 0x000000020,
    }
    #endregion

    public class SystemImageList : IDisposable
    {
        public static IntPtr Test()
        {
            var shfi = new NativeMethods.SHFILEINFO();
            var shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
            return NativeMethods.SHGetFileInfo(@"C:\", 16, ref shfi, shfiSize, 16384);
        }

        #region Constructor

        public SystemImageList(IconSize size)
        {
            if (!IsXpOrAbove())
                throw new NotSupportedException("Windows XP or above required.");

            _size = size == IconSize.Thumbnail ? IconSize.Jumbo : size; //There is no thumbnail mode in shell.

            if (!IsVistaUp() && (_size == IconSize.Jumbo || _size == IconSize.ExtraLarge)) //XP do not have extra large or jumbo.
                _size = IconSize.Large;

            var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            var hr = NativeMethods.SHGetImageList((int)_size, ref iidImageList, ref _iImageList); // Get the System IImageList object from the Shell:
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
            // the image list handle is the IUnknown pointer, but using Marshal.GetIUnknownForObject doesn't return
            // the right value.  It really doesn't hurt to make a second call to get the handle:            
            NativeMethods.SHGetImageListHandle((int)_size, ref iidImageList, ref _ptrImageList);


            //int cx = 0, cy = 0;
            //ImageList_GetIconSize(_ptrImageList, ref cx, ref cy);
            //Debug.WriteLine(cx);

            //_iImageList.SetImageCount(2);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SystemImageList()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_iImageList != null)
                        Marshal.ReleaseComObject(_iImageList);
                    _iImageList = null;
                }
            }
            _disposed = true;
        }
        #endregion

        #region Methods

        private static NativeMethods.IImageList GetImageListInterface(IconSize size)
        {
            NativeMethods.IImageList iImageList = null;
            var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            var hr = NativeMethods.SHGetImageList((int)size, ref iidImageList, ref iImageList); // Get the System IImageList object from the Shell:
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
            return iImageList;
        }

        public Bitmap this[string fileName, bool isDirectory, bool forceLoadFromDisk, ShellIconStateConstants iconState]
        {
            get
            {
                try
                {
                    var icon = GetIcon(GetIconIndex(fileName, isDirectory, forceLoadFromDisk, iconState));
                    return icon == null ? new Bitmap(1, 1) : icon.ToBitmap();
                }
                catch { return new Bitmap(1, 1); }
            }
        }

        public Bitmap this[IntPtr pidlPtr, bool isDirectory, bool forceLoadFromDisk, ShellIconStateConstants iconState]
        {
            get
            {
                try
                {
                    var icon = GetIcon(GetIconIndex(pidlPtr, isDirectory, forceLoadFromDisk, iconState));
                    return icon == null ? new Bitmap(1, 1) : icon.ToBitmap();
                }
                catch { return new Bitmap(1, 1); }
            }
        }

        public Bitmap this[string fileName, bool isDirectory, bool forceLoadFromDisk]
        {
            get { return this[fileName, isDirectory, forceLoadFromDisk, ShellIconStateConstants.ShellIconStateNormal]; }
        }

        public Bitmap this[IntPtr pidlPtr, bool isDirectory, bool forceLoadFromDisk]
        {
            get { return this[pidlPtr, isDirectory, forceLoadFromDisk, ShellIconStateConstants.ShellIconStateNormal]; }
        }

        public Bitmap this[string fileName, bool isDirectory]
        {
            get { return this[fileName, isDirectory, false, ShellIconStateConstants.ShellIconStateNormal]; }
        }

        public Bitmap this[IntPtr pidlPtr, bool isDirectory]
        {
            get { return this[pidlPtr, isDirectory, false, ShellIconStateConstants.ShellIconStateNormal]; }
        }

        private void GetAttributes(bool isDirectory, bool forceLoadFromDisk, out uint dwAttr, out NativeMethods.SHGetFileInfoConstants dwFlags)
        {
            dwFlags = NativeMethods.SHGetFileInfoConstants.SHGFI_SYSICONINDEX;
            dwAttr = 0;

            if (_size == IconSize.Small)
                dwFlags |= NativeMethods.SHGetFileInfoConstants.SHGFI_SMALLICON;

            if (isDirectory)
            {
                dwAttr = NativeMethods.FILE_ATTRIBUTE_DIRECTORY;
            }
            else
                if (!forceLoadFromDisk)
                {
                    dwFlags |= NativeMethods.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES;
                    dwAttr = NativeMethods.FILE_ATTRIBUTE_NORMAL;
                }
        }

        private int GetIconIndex(string fileName, bool isDirectory, bool forceLoadFromDisk, ShellIconStateConstants iconState)
        {
            NativeMethods.SHGetFileInfoConstants dwFlags; uint dwAttr;
            GetAttributes(isDirectory, forceLoadFromDisk, out dwAttr, out dwFlags);

            // sFileSpec can be any file.

            if (fileName.EndsWith(".lnk", StringComparison.InvariantCultureIgnoreCase))
            {
                dwFlags |= NativeMethods.SHGetFileInfoConstants.SHGFI_LINKOVERLAY | NativeMethods.SHGetFileInfoConstants.SHGFI_ICON;
                iconState = ShellIconStateConstants.ShellIconStateLinkOverlay;
                forceLoadFromDisk = true;
            }

            var shfi = new NativeMethods.SHFILEINFO();
            var shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
            var retVal = NativeMethods.SHGetFileInfo(fileName, dwAttr, ref shfi, shfiSize, ((uint)(dwFlags) | (uint)iconState));

            if (!retVal.Equals(IntPtr.Zero)) return shfi.iIcon.ToInt32();
            if (forceLoadFromDisk)
                return GetIconIndex(Path.GetFileName(fileName), isDirectory, false, iconState);
            Debug.Assert((!retVal.Equals(IntPtr.Zero)), "Failed to get icon index");
            return -1;
        }
        private int GetIconIndex(IntPtr pidlPtr, bool isDirectory, bool forceLoadFromDisk, ShellIconStateConstants iconState)
        {
            NativeMethods.SHGetFileInfoConstants dwFlags; uint dwAttr;
            GetAttributes(isDirectory, forceLoadFromDisk, out dwAttr, out dwFlags);
            dwFlags |= NativeMethods.SHGetFileInfoConstants.SHGFI_PIDL;

            var shfi = new NativeMethods.SHFILEINFO();
            var shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
            var retVal = NativeMethods.SHGetFileInfo(pidlPtr, dwAttr, ref shfi, shfiSize, ((uint)(dwFlags) | (uint)iconState));

            if (!retVal.Equals(IntPtr.Zero)) return shfi.iIcon.ToInt32();
            Debug.Assert((!retVal.Equals(IntPtr.Zero)), "Failed to get icon index");
            return -1;
        }
        private Icon GetIcon(int index)
        {
            if (index == -1) return null;

            Icon icon = null;
            var hIcon = NativeMethods.ImageList_GetIcon(_ptrImageList, index,
                                                        (int)(ImageListDrawItemConstants.ILD_TRANSPARENT | ImageListDrawItemConstants.ILD_SCALE));

            if (hIcon != IntPtr.Zero)
            {
                icon = Icon.FromHandle(hIcon);
            }
            return icon != null ? icon.Clone() as Icon : null;

        }

        private Bitmap GetBitmap(int index, ImageListDrawItemConstants flags)
        {
            var bitmapSize = GetImageListIconSize();
            var bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height);

            using (var g = Graphics.FromImage(bitmap))
            {
                try
                {
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, bitmapSize.Width, bitmapSize.Height));

                    var hdc = g.GetHdc();

                    var pimldp = new NativeMethods.IMAGELISTDRAWPARAMS { hdcDst = hdc };
                    pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
                    pimldp.i = index;
                    pimldp.x = 0;
                    pimldp.y = 0;
                    pimldp.cx = bitmapSize.Width;
                    pimldp.cy = bitmapSize.Height;
                    pimldp.fStyle = (int)flags;

                    if (_iImageList == null || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
                    {
                        NativeMethods.ImageList_DrawIndirect(ref pimldp);
                    }
                    else
                    {
                        _iImageList.Draw(ref pimldp);
                    }
                }
                finally
                {
                    g.ReleaseHdc();
                }


            }

            bitmap.MakeTransparent();
            return bitmap;
        }


        private Bitmap GetBitmap(int index)
        {
            var normal = GetBitmap(index, ImageListDrawItemConstants.ILD_TRANSPARENT
                | ImageListDrawItemConstants.ILD_IMAGE | ImageListDrawItemConstants.ILD_SCALE);

            return normal;
        }



        private static bool IsXpOrAbove()
        {
            var ret = false;
            if (Environment.OSVersion.Version.Major > 5)
            {
                ret = true;
            }
            else if ((Environment.OSVersion.Version.Major == 5) &&
                (Environment.OSVersion.Version.Minor >= 1))
            {
                ret = true;
            }
            return ret;
            //return false;
        }

        private static bool IsVistaUp()
        {
            return (Environment.OSVersion.Version.Major >= 6);
        }

        private Size GetImageIconSize(int index)
        {
            var imgInfo = new NativeMethods.IMAGEINFO();
            int hr;

            if (_iImageList == null || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
                hr = NativeMethods.ImageList_GetImageInfo(_ptrImageList, index, ref imgInfo);
            else hr = _iImageList.GetImageInfo(index, ref imgInfo);

            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            var rect = imgInfo.rcImage;
            return new Size(rect.right - rect.left, rect.bottom - rect.top);
        }



        private Size GetImageListIconSize()
        {
            int cx = 0, cy = 0;
            if (_iImageList == null || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
                NativeMethods.ImageList_GetIconSize(_ptrImageList, ref cx, ref cy);
            else _iImageList.GetIconSize(ref cx, ref cy);
            return new Size(cx, cy);
        }

        #endregion

        #region Data

        private IntPtr _ptrImageList = IntPtr.Zero;
        private NativeMethods.IImageList _iImageList;
        private bool _disposed;
        private IconSize _size;
        #endregion



    }
}