﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkyJukebox.Icons
{
    public class EmbeddedPngIcon : Icon
    {
        public EmbeddedPngIcon(string path) : base(path) { }

        private Image _image;
        public override Image GetImage()
        {
            if (_image != null) return _image;
            var ms = new MemoryStream();
            var bbe = new PngBitmapEncoder();
            bbe.Frames.Add(BitmapFrame.Create(new Uri(Path)));
            bbe.Save(ms);
            return _image = Image.FromStream(ms);
        }

        private ImageSource _imageSource;
        public override ImageSource GetImageSource()
        {
            return _imageSource ?? (_imageSource = new BitmapImage(new Uri(Path)));
        }
        public override void SetRecolor(System.Drawing.Color c)
        {
            if (IsRecolored) _image = null;
            if (_image == null) GetImage();
            IsRecolored = true;
            _image = _image.RecolorFromGrayscale(c);
            _imageSource = _image.ToBitmapSource();
        }

        public override void ResetColor()
        {
            _image = null;
            _imageSource = null;
            IsRecolored = false;
        }
    }
}
