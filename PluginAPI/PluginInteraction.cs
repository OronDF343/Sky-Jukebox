using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SkyJukebox.CoreApi.Contracts;
using SkyJukebox.CoreApi.Playback;
using SkyJukebox.CoreApi.Utils;

namespace SkyJukebox.CoreApi
{
    public static class PluginInteraction
    {
        private static IEnumerable<T> GetPlugins<T>(string path)
        {
            // If this works, then this is some of my favorite code ^_^
            if (!typeof(T).IsInterface) return null;
            return from dllFile in Directory.GetFiles(path, "*.dll")
                   let a = Assembly.Load(AssemblyName.GetAssemblyName(dllFile))
                   where a != null
                   from t in a.GetTypes()
                   let pluginType = typeof(T)
                   where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                   select (T)Activator.CreateInstance(t);
        }

        public static void RegisterAllPlugins()
        {
            // Load plugins
            //InstanceManager.LoadedPlugins = GetPlugins<IPlugin>(StringUtils.GetExePath());
            //TODO: fix this!

            // Register external AudioPlayers
            foreach (var a in GetPlugins<IAudioPlayer>(StringUtils.GetExePath()))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
        }
    }
}
