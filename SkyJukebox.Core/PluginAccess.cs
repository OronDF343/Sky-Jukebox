using System;
using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Lib;

namespace SkyJukebox.Core
{
    public class PluginAccess : IPluginAccess
    {
        public IPlaybackManager GetPlaybackManager()
        {
            return PlaybackManager.Instance;
        }

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
    }
}
