using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ExifLib;

// Original version of this code (c) Leung Yat Chun Joseph - licensed under MIT

namespace SkyJukebox.Lib.TreeBrowser
{
    public class IconExtractor
    {
        private const string ImageFilter = ".jpg,.jpeg,.png,.gif,.bmp,.tiff";
        private const string SpecialFilter = ".exe,.lnk";
        protected static readonly string TempPath = Path.GetTempPath();

        #region Static Methods

        #region IconSize Utils
        public static Size IconSizeToSize(IconSize size)
        {
            switch (size)
            {
                case IconSize.Thumbnail: return new Size(128, 128);
                case IconSize.Jumbo: return new Size(80, 80);
                case IconSize.ExtraLarge: return new Size(64, 64);
                case IconSize.Large: return new Size(32, 32);
                default: return new Size(16, 16);
            }
        }

        public static IconSize SizeToIconSize(int size)
        {
            if (size <= 16) return IconSize.Small;
            if (size <= 32) return IconSize.Large;
            if (size <= 47) return IconSize.ExtraLarge;
            //if (iconSize <= 72) return IconSize.jumbo;
            return IconSize.Thumbnail;
        }

        #endregion

        /// <summary>
        /// Return Exif thumbnail for jpegs.
        /// </summary>
        /// <param name="fileName">Jpeg filename</param>
        /// <returns>Bitmap, null if anything goes wrong</returns>
        protected static Bitmap GetExifThumbnail(string fileName)
        {
            var s = Path.GetExtension(fileName);
            if (s == null || !File.Exists(fileName)) return null;
            var ext = s.ToLower();
            if (!IsJpeg(ext)) return null;
            var reader = new ExifReader(fileName);
            var bitmapBytes = reader.GetJpegThumbnailBytes();
            return bitmapBytes != null && bitmapBytes.Length > 0 ? new Bitmap(new MemoryStream(bitmapBytes)) : null;
        }

        protected static bool IsJpeg(string ext)
        {
            if (!(ext.StartsWith(".")))
                ext = Path.GetExtension(ext);
            if (String.IsNullOrEmpty(ext)) return false;

            ext = ext.ToLower();
            return ext == ".jpg" || ext == ".jpeg";
        }

        protected static bool IsSpecialIcon(string ext)
        {
            if (!(ext.StartsWith(".")))
                ext = Path.GetExtension(ext);
            if (String.IsNullOrEmpty(ext)) return false;

            return SpecialFilter.IndexOf(ext.ToLower(), StringComparison.Ordinal) != -1;
        }

        protected static bool IsImageIcon(string ext)
        {
            if (!(ext.StartsWith(".")))
                ext = Path.GetExtension(ext);
            if (String.IsNullOrEmpty(ext)) return false;

            return ImageFilter.IndexOf(ext.ToLower(), StringComparison.Ordinal) != -1;
        }
        #endregion

        #region Methods

        public static Bitmap GetBitmap(IconSize size, IntPtr ptr, bool isDirectory, bool forceLoad)
        {
            Bitmap retVal;
            using (var imgList = new SystemImageList(size))
                retVal = imgList[ptr, isDirectory, forceLoad];
            return retVal;
        }

        public static Bitmap GetBitmap(IconSize size, string fileName, bool isDirectory, bool forceLoad)
        {
            Bitmap retVal;
            using (var imgList = new SystemImageList(size))
                retVal = imgList[fileName, isDirectory, forceLoad];
            return retVal;
        }

        public Bitmap GetFileBasedFsBitmap(string ext, IconSize size)
        {
            var lookup = TempPath;
            var folderBitmap = GetGenericIcon(lookup, size, true);
            if (ext == "") return folderBitmap;
            ext = ext.Substring(0, 1).ToUpper() + ext.Substring(1).ToLower();

            using (var g = Graphics.FromImage(folderBitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                var font = new Font("Comic Sans MS", Math.Max(folderBitmap.Width / 5, 1), FontStyle.Bold | FontStyle.Italic);
                var height = g.MeasureString(ext, font).Height;
                var rightOffset = folderBitmap.Width / 5;

                if (size == IconSize.Small)
                {
                    font = new Font("Arial", 5, FontStyle.Bold);
                    height = g.MeasureString(ext, font).Height;
                    rightOffset = 0;
                }

                g.DrawString(ext, font,
                             Brushes.Black,
                             new RectangleF(0, folderBitmap.Height - height, folderBitmap.Width - rightOffset, height),
                             new StringFormat(StringFormatFlags.DirectionRightToLeft));

            }

            return folderBitmap;
        }

        protected static Bitmap GetGenericIcon(string fullPathOrExt, IconSize size, bool isFolder = false, bool forceLoad = false)
        {
            try
            {
                var fileName = fullPathOrExt.StartsWith(".") ? "AAA" + fullPathOrExt : fullPathOrExt;

                switch (size)
                {
                    case IconSize.Thumbnail:
                    case IconSize.ExtraLarge:
                    case IconSize.Jumbo:
                        Bitmap retImage = null;

                        try
                        {
                            retImage = GetBitmap(size, fileName, isFolder, forceLoad);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("GetGenericIcon - " + ex.Message);
                            //TO-DO: Fix exception:
                            //GetGenericIcon Unable to cast COM object of type 'System.__ComObject' to interface type 'IImageList'. 
                            //This operation failed because the QueryInterface call on the COM component for the interface with IID 
                            //'{46EB5926-582E-4017-9FDF-E8998DAA0950}' failed due to the following error: No such interface supported 
                            //(Exception from HRESULT: 0x80004002 (E_NOINTERFACE)).

                            //FailSafe
                            if (size > IconSize.Large)
                                return GetGenericIcon(fullPathOrExt, IconSize.Large, isFolder, forceLoad);
                        }

                        return ImageTools.ResizeImage(ImageTools.CheckImage(retImage) ? retImage : ImageTools.CutImage(retImage, new Size(48, 48)), IconSizeToSize(size), 0);
                }

                var shinfo = new NativeMethods.SHFILEINFO();

                var flags = NativeMethods.SHGFI_SYSICONINDEX;
                if (!isFolder)
                    flags |= NativeMethods.SHGFI_USEFILEATTRIBUTES;

                if (size == IconSize.Small)
                    flags = flags | NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON;
                else flags = flags | NativeMethods.SHGFI_ICON;
                try
                {
                    NativeMethods.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetGenericIcon - " + ex.Message);
                    return new Bitmap(1, 1);
                }
                if (shinfo.hIcon == IntPtr.Zero) return new Bitmap(1, 1);
                var retVal = Icon.FromHandle(shinfo.hIcon).ToBitmap();
                NativeMethods.DestroyIcon(shinfo.hIcon);
                return retVal;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetGenericIcon - " + ex.Message);
                return new Bitmap(1, 1);
            }
        }

        protected Bitmap GetGenericIcon(IntPtr ptr, IconSize size, bool isFolder = false, bool forceLoad = false)
        {
            switch (size)
            {
                case IconSize.Thumbnail:
                case IconSize.ExtraLarge:
                case IconSize.Large:
                case IconSize.Jumbo:
                    var retImage = GetBitmap(size, ptr, isFolder, forceLoad);
                    return ImageTools.ResizeImage(ImageTools.CheckImage(retImage) ? retImage : ImageTools.CutImage(retImage, new Size(48, 48)), IconSizeToSize(size), 0);
            }

            var shinfo = new NativeMethods.SHFILEINFO();

            var flags = NativeMethods.SHGFI_SYSICONINDEX | NativeMethods.SHGFI_PIDL;
            if (!isFolder)
                flags |= NativeMethods.SHGFI_USEFILEATTRIBUTES;

            if (size == IconSize.Small)
                flags = flags | NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON;
            else flags = flags | NativeMethods.SHGFI_ICON;
            try
            {
                NativeMethods.SHGetFileInfo(ptr, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            }
            catch
            {
                return new Bitmap(1, 1);
            }

            if (shinfo.hIcon == IntPtr.Zero) return new Bitmap(1, 1);
            var retVal = Icon.FromHandle(shinfo.hIcon).ToBitmap();
            NativeMethods.DestroyIcon(shinfo.hIcon);
            return retVal;
        }
        #endregion
    }

    /// <summary>
    /// .Net 2.0 WinForms level icon extractor with cache support.
    /// </summary>
    /// <typeparam name="TFsi"></typeparam>
    public abstract class IconExtractor<TFsi> : IconExtractor //T may be FileSystemInfo, Ex or ExA
    {
        #region Data

        readonly Dictionary<Tuple<string, IconSize>, Bitmap> _iconCache = new Dictionary<Tuple<string, IconSize>, Bitmap>();
        readonly ReaderWriterLock _iconCacheLock = new ReaderWriterLock();

        #endregion

        #region Constructor

        public IconExtractor()
        {
            InitCache();
        }

        #endregion

        #region Methods

        protected abstract void GetIconKey(TFsi entry, IconSize size, out string fastKey, out string slowKey);
        protected abstract Bitmap GetIconInner(TFsi entry, string key, IconSize size);

        protected void InitCache()
        {
            Action<IconSize> addToDic = size =>
            {
                var iconKey = new Tuple<string, IconSize>(TempPath, size);
                _iconCache.Add(iconKey, GetGenericIcon(TempPath, size, true));
            };

            lock (_iconCache)
                foreach (IconSize size in Enum.GetValues(typeof(IconSize)))
                    addToDic(size);
        }

        public bool IsDelayLoading(TFsi entry, IconSize size)
        {
            string fastKey, slowKey;
            GetIconKey(entry, size, out fastKey, out slowKey);
            return fastKey != slowKey;
        }

        public Bitmap GetIcon(TFsi entry, string key, bool isDir, IconSize size)
        {
            Func<string, IconSize, Bitmap> getIconFromCache =
                (k, s) =>
                {
                    var dicKey = new Tuple<string, IconSize>(k, s);

                    try
                    {
                        _iconCacheLock.AcquireReaderLock(0);

                        if (_iconCache.ContainsKey(dicKey))
                            lock (_iconCache[dicKey])
                                return _iconCache[dicKey];
                    }
                    catch { return null; }
                    finally { _iconCacheLock.ReleaseReaderLock(); }

                    return null;
                };

            Action<string, IconSize, Bitmap> addIconToCache =
               (k, s, b) =>
               {
                   var dicKey = new Tuple<string, IconSize>(k, s);

                   if (!k.StartsWith(".")) return;
                   try
                   {
                       _iconCacheLock.AcquireWriterLock(Timeout.Infinite);
                       if (!_iconCache.ContainsKey(dicKey))
                           _iconCache.Add(dicKey, b);
                       else _iconCache[dicKey] = b;
                   }
                   finally { _iconCacheLock.ReleaseWriterLock(); }
               };

            var retImg = getIconFromCache(key, size);
            if (retImg != null) return retImg;

            try
            {
                if (key.StartsWith(".")) //ext, retrieve automatically
                    retImg = GetGenericIcon(key, size);
                else
                    if (IsSpecialIcon(key) && File.Exists(key))
                        retImg = GetGenericIcon(key, size, isDir, true);
                    else
                        retImg = GetIconInner(entry, key, size);
            }
            catch (Exception ex)
            { retImg = null; Debug.WriteLine("IconExtractor.GetIcon" + ex.Message); }

            if (retImg == null) return null;
            var destSize = IconSizeToSize(size);

            if (size == IconSize.Jumbo && IsImageIcon(key))
                retImg = ImageTools.ResizeImage(retImg, destSize, 5);
            else retImg = ImageTools.ResizeImage(retImg, destSize, 0);

            addIconToCache(key, size, retImg);

            return retImg;
        }

        public Bitmap GetIcon(string fileName, IconSize size, bool isDir)
        {
            return GetBitmap(size, fileName, isDir, true);
        }

        public Bitmap GetIcon(TFsi entry, IconSize size, bool isDir, bool fast)
        {
            string fastKey, slowKey;
            GetIconKey(entry, size, out fastKey, out slowKey);

            if (fast || size <= IconSize.Large)
                return GetIcon(entry, fastKey, isDir, size);
            var icon = GetIcon(entry, slowKey, isDir, size);
            return icon;
        }

        #endregion



    }
}