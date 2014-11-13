using System.Collections.Generic;
using System.Linq;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Core
{
    public static class PluginInteraction
    {
        public static IEnumerable<IPlugin> RegisterAllExtensions()
        {
            // Register AudioPlayers
            foreach (var a in AssemblyLoader.GetExtensions<IAudioPlayer>(StringUtils.GetExePath()))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
            // Register plugins
            return AssemblyLoader.GetExtensions<IPlugin>(StringUtils.GetExePath());
        }
    }
}
