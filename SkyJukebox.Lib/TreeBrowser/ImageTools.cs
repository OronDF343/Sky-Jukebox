using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

// Original version of this code (c) Leung Yat Chun Joseph - licensed under MIT

namespace SkyJukebox.Lib.TreeBrowser
{
    public static class ImageTools
    {
        #region Image Tools
        public static BitmapSource LoadBitmap(Bitmap source)
        {
            if (source == null)
                source = new Bitmap(1, 1);
            var ms = new MemoryStream();
            lock (source)
                source.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        /// <summary>
        /// Check if a jumbo icon is actually a 32x32 icon.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static bool CheckImage(Bitmap bitmap)
        {
            var centre = new Point(bitmap.Width / 2, bitmap.Height / 2);
            return bitmap.GetPixel(centre.X, centre.Y) != Color.FromArgb(0, 0, 0, 0);
        }

        public static void ClearBackground(WriteableBitmap target, bool dispatcher)
        {
            var bitmap = new Bitmap((int)target.Width, (int)target.Height);
            using (var g = Graphics.FromImage(bitmap))
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            CopyBitmap(LoadBitmap(bitmap), target, dispatcher, 0, false);
        }

        public static void CopyBitmap(BitmapSource source, WriteableBitmap target, bool dispatcher, int spacing, bool freezeBitmap)
        {
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var stride = width * ((source.Format.BitsPerPixel + 7) / 8);

            var bits = new byte[height * stride];
            source.CopyPixels(bits, stride, 0);

            if (dispatcher)
            {
                target.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new ThreadStart(delegate
                {
                    //UI Thread
                    var delta = target.Height - height;
                    var newWidth = width > target.Width ? (int)target.Width : width;
                    var outRect = new Int32Rect((int)((target.Width - newWidth) / 2), (int)(delta >= 0 ? delta : 0) / 2 + spacing, newWidth - (spacing * 2), newWidth - (spacing * 2));
                    try
                    {
                        target.WritePixels(outRect, bits, stride, 0);
                        if (freezeBitmap)
                        {
                            target.Freeze();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        Debugger.Break();
                    }
                }));
            }
            else
            {
                var delta = target.Height - height;
                var newWidth = width > target.Width ? (int)target.Width : width;
                var outRect = new Int32Rect(spacing, (int)(delta >= 0 ? delta : 0) / 2 + spacing, newWidth - (spacing * 2), newWidth - (spacing * 2));
                try
                {
                    target.WritePixels(outRect, bits, stride, 0);
                    if (freezeBitmap)
                        target.Freeze();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Debugger.Break();
                }
            }
        }

        public static Bitmap CutImage(Bitmap imgToCut, Size size)
        {

            var b = new Bitmap(size.Width, size.Height);
            var rect = new RectangleF(0, 0, size.Width, size.Height);

            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.High;


            g.DrawImage(imgToCut, rect, rect, GraphicsUnit.Pixel);
            g.Dispose();

            return b;
        }

        //http://blog.paranoidferret.com/?p=11 , modified a little.
        public static Bitmap ResizeImage(Bitmap imgToResize, Size size, int spacing)
        {
            lock (imgToResize)
            {
                if (imgToResize.Width == size.Width && imgToResize.Height == size.Height && spacing == 0)
                    return imgToResize;

                var sourceWidth = imgToResize.Width;
                var sourceHeight = imgToResize.Height;

                if ((sourceWidth == size.Width) && (sourceHeight == size.Height))
                    return imgToResize;


                var nPercentW = (size.Width / (float)sourceWidth);
                var nPercentH = (size.Height / (float)sourceHeight);

                var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

                var destWidth = (int)((sourceWidth * nPercent) - spacing * 4);
                var destHeight = (int)((sourceHeight * nPercent) - spacing * 4);

                var leftOffset = (size.Width - destWidth) / 2;
                var topOffset = (size.Height - destHeight) / 2;


                var b = new Bitmap(size.Width, size.Height);
                using (var g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.High;
                    if (spacing > 0)
                    {
                        g.DrawLines(Pens.Silver, new PointF[]
                        {
                            new PointF(leftOffset - spacing, topOffset + destHeight + spacing), //BottomLeft
                            new PointF(leftOffset - spacing, topOffset - spacing), //TopLeft
                            new PointF(leftOffset + destWidth + spacing, topOffset - spacing) //TopRight
                        });

                        g.DrawLines(Pens.Gray, new PointF[]
                        {
                            new PointF(leftOffset + destWidth + spacing, topOffset - spacing), //TopRight
                            new PointF(leftOffset + destWidth + spacing, topOffset + destHeight + spacing), //BottomRight
                            new PointF(leftOffset - spacing, topOffset + destHeight + spacing) //BottomLeft
                        });
                    }

                    g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
                }
                return b;

            }
        }
        public static Bitmap ResizeJumbo(Bitmap imgToResize, Size size, int spacing)
        {
            lock (imgToResize)
            {
                if (imgToResize.Width == size.Width && imgToResize.Height == size.Height && spacing == 0)
                    return imgToResize;

                const int destWidth = 80;
                const int destHeight = 80;

                var leftOffset = (size.Width - destWidth) / 2;
                var topOffset = (size.Height - destHeight) / 2;


                var b = new Bitmap(size.Width, size.Height);
                using (var g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.High;
                    g.DrawLines(Pens.Silver, new PointF[]
                    {
                        new PointF(0 + spacing, size.Height - spacing), //BottomLeft
                        new PointF(0 + spacing, 0 + spacing), //TopLeft
                        new PointF(size.Width - spacing, 0 + spacing) //TopRight
                    });

                    g.DrawLines(Pens.Gray, new PointF[]
                    {
                        new PointF(size.Width - spacing, 0 + spacing), //TopRight
                        new PointF(size.Width - spacing, size.Height - spacing), //BottomRight
                        new PointF(0 + spacing, size.Height - spacing) //BottomLeft
                    });


                    g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
                    //   g.Dispose();
                }

                return b;
            }
        }
        #endregion

    }
}