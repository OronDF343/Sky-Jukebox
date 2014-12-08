using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;

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
            return new EmbeddedPngIcon(path);
        }
    }
}
