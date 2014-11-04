using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SkyJukebox.Api;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Utils;

namespace SkyJukebox.Core
{
    public static class PluginInteraction
    {
        public static IEnumerable<IPlugin> RegisterAllPlugins()
        {
            // Register AudioPlayers
            foreach (var a in Lib.Utils.GetPlugins<IAudioPlayer>(StringUtils.GetExePath()))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
            // Register plugins
            return Lib.Utils.GetPlugins<IPlugin>(StringUtils.GetExePath());
        }
    }
}
