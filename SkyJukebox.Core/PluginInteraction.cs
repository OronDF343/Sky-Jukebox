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
            foreach (var a in AssemblyLoader.GetExtensions<IAudioPlayer, ExtensionAttribute>(StringUtils.GetExePath()))
            {
                // TODO: check the version and stuff
                var e = from x in a.Instance.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a.Instance);
            }
            // Register plugins
            return AssemblyLoader.GetExtensions<IPlugin>(StringUtils.GetExePath());
        }
    }
}
