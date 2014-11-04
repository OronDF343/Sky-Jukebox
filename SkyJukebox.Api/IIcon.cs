﻿using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace SkyJukebox.Api
{
    public interface IIcon
    {
        string Path { get; }
        Image Image { get; }

        ImageSource ImageSource { get; }
        bool IsRecolored { get; }
        void SetRecolor(Color c);
        void ResetColor();
    }
}
