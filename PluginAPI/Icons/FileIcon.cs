using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkyJukebox.CoreApi.Contracts;
using SkyJukebox.CoreApi.Utils;
using Color = System.Drawing.Color;

namespace SkyJukebox.CoreApi.Icons
{
    public class FileIcon : IIcon
    {
        public FileIcon(string path)
        {
            Path = path;
        }
        public string Path { get; private set; }

        private Image _image;
        public Image GetImage()
        {
            return _image ?? (_image = new Bitmap(Path));
        }

        private ImageSource _imageSource;
        public ImageSource GetImageSource()
        {
            return _imageSource ?? (_imageSource = new BitmapImage(new Uri(Path)));
        }

        public bool IsRecolored { get; private set; }
        public void SetRecolor(Color c)
        {
            _imageSource = null;
            if (IsRecolored) _image = null;
            if (_image == null) GetImage();
            else IsRecolored = true;
            _image = _image.RecolorFromGrayscale(c);
            _imageSource = _image.ToBitmapSource();
        }

        public void ResetColor()
        {
            _image = null;
            _imageSource = null;
            IsRecolored = false;
        }
    }
}
