using System.Collections.Generic;
using System.Linq;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Lib.Plugins;

namespace SkyJukebox.Core
{
    public static class PluginInteraction
    {
        public static IEnumerable<IPlugin> RegisterAllPlugins()
        {
            // Register AudioPlayers
            foreach (var a in AssemblyLoader.GetPlugins<IAudioPlayer>(StringUtils.GetExePath()))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
            // Register plugins
            return AssemblyLoader.GetPlugins<IPlugin>(StringUtils.GetExePath());
        }
    }
}
