using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SkyJukebox.Icons
{
    public abstract class Icon
    {
        protected Icon() { }
        public Icon(string path)
        {
            Path = path;
        }
        public string Path { get; private set; }
        public abstract Image GetImage();
        public abstract ImageSource GetImageSource();
        public bool IsRecolored { get; set; }
        public abstract void SetRecolor(System.Drawing.Color c);
        public abstract void ResetColor();
    }
}
