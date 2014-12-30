﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SkyJukebox.Lib.Icons
{
    public class EmbeddedGifIcon : IconBase
    {
        public EmbeddedGifIcon(string path)
        {
            Path = path;
        }

        public override Image Image
        {
            get
            {
                if (InnerImage != null) return InnerImage;
                var ms = new MemoryStream();
                var bbe = new GifBitmapEncoder();
                bbe.Frames.Add(BitmapFrame.Create(new Uri(Path)));
                bbe.Save(ms);
                return InnerImage = Image.FromStream(ms);
            }
        }
    }
}
