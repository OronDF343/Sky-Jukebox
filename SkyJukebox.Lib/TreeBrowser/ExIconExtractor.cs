using System;
using System.Drawing;
using System.IO;

// Original version of this code (c) Leung Yat Chun Joseph - licensed under MIT

namespace SkyJukebox.Lib.TreeBrowser
{
    public class ExIconExtractor : IconExtractor<FileSystemInfoEx>
    {
        #region Methods

        private static bool IsGuidPath(string fullName)
        {
            return fullName.StartsWith("::{");
        }

        protected override Bitmap GetIconInner(FileSystemInfoEx entry, string key, IconSize size)
        {
            if (key.StartsWith("."))
                throw new Exception("ext item is handled by IconExtractor");

            if (!(entry is FileInfoEx))
                return entry.RequestPIDL(pidl => GetBitmap(size, pidl.Ptr, entry is DirectoryInfoEx, false));
            Bitmap retVal = null;

            var ext = PathEx.GetExtension(entry.Name);
            if (IsJpeg(ext))
            {
                retVal = GetExifThumbnail(entry.FullName);
            }
            if (!IsImageIcon(ext))
                return retVal ?? entry.RequestPIDL(pidl => GetBitmap(size, pidl.Ptr, entry is DirectoryInfoEx, false));
            try
            {
                retVal = new Bitmap(entry.FullName);
            }
            catch { retVal = null; }

            return retVal ?? entry.RequestPIDL(pidl => GetBitmap(size, pidl.Ptr, entry is DirectoryInfoEx, false));
        }

        protected override void GetIconKey(FileSystemInfoEx entry, IconSize size, out string fastKey, out string slowKey)
        {
            var ext = PathEx.GetExtension(entry.Name);
            if (entry is DirectoryInfoEx)
            {
                fastKey = entry.FullName;
                slowKey = entry.FullName;
            }

            else
                if (IsGuidPath(entry.Name))
                {
                    fastKey = entry.FullName;
                    slowKey = entry.FullName;
                }
                else
                    if (IsImageIcon(ext) || IsSpecialIcon(ext))
                    {
                        fastKey = ext;
                        slowKey = entry.FullName;
                    }
                    else
                    {
                        fastKey = slowKey = ext;
                    }

        }

        #endregion
    }
}