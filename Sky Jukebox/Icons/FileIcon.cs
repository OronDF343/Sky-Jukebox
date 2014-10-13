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
    public class FileIcon : Icon
    {
        public FileIcon(string path) : base(path) { }

        private Image _image;
        public override Image GetImage()
        {
            return _image ?? (_image = new Bitmap(Path));
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
            else IsRecolored = true;
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
