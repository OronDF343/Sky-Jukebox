using System;
using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib;

namespace SkyJukebox.Core
{
    public class PluginAccess : IPluginAccess
    {
        public IIcon CreateFileIcon(string path)
        {
            return new FileIcon(path);
        }

        public IIcon CreateEmbeddedIcon(string path)
        {
            switch (path.GetExt())
            {
                case "png":
                    return new EmbeddedPngIcon(path);
                case "bmp":
                    return new EmbeddedBmpIcon(path);
                case "tif":
                case "tiff":
                    return new EmbeddedTiffIcon(path);
                case "gif":
                    return new EmbeddedGifIcon(path);
                case "jpg":
                case "jpeg":
                    return new EmbeddedJpegIcon(path);
                default:
                    throw new InvalidOperationException("Usupported image format!");
            }
        }

        public IPlaybackManager PlaybackManagerInstance { get { return PlaybackManager.Instance; } }


        public ISettingsManager SettingsManagerInstance { get { return SettingsManager.Instance; } }
    }
}
