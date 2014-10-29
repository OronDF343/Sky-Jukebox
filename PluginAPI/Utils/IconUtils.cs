using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SkyJukebox.CoreApi.Utils
{
    public static class IconUtils
    {
        public static Bitmap RecolorFromGrayscale(this Image img, Color color)
        {
            var cm = new ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {color.R / 255F, color.G / 255F, color.B / 255F, 0, 1}
            });

            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            var bmp = new Bitmap(img.Width, img.Height);
            var gfx = Graphics.FromImage(bmp);
            var rect = new Rectangle(0, 0, img.Width, img.Height);

            gfx.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

            return bmp;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Image source)
        {
            var bitmap = new Bitmap(source);

            var bitSrc = bitmap.ToBitmapSource();

            bitmap.Dispose();
            bitmap = null;

            return bitSrc;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            BitmapSource bitSrc;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        public static System.Windows.Media.Color ToWpfColor(this System.Drawing.Color c)
        {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
