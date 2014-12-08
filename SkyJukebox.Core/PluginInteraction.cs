using System.Collections.Generic;
using System.Linq;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Core
{
    public static class PluginInteraction
    {
        public static IEnumerable<ExtensionInfo<IPlugin>> RegisterAllExtensions()
        {
            // Register AudioPlayers
            foreach (var a in ExtensionLoader.GetCompatibleExtensions<IAudioPlayer>(Lib.Utils.GetExePath()))
            {
                var e = from x in a.Instance.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a.Instance);
            }
            // Register plugins
            return ExtensionLoader.GetCompatibleExtensions<IPlugin>(Lib.Utils.GetExePath());
        }
    }
}
