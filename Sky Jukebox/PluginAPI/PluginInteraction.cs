using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using SkyJukebox.Playback;

namespace SkyJukebox.PluginAPI
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
            Instance.LoadedPlugins = GetPlugins<IPlugin>(Instance.ExePath);

            // Load built-in NAudio codecs
            NAudioPlayer.AddCodec(new string[] { "mp3", "wav", "m4a", "aac", "aiff", "mpc", "ape" }, typeof(AudioFileReader));
            NAudioPlayer.AddCodec(new string[] { "wma" }, typeof(WMAFileReader));
            NAudioPlayer.AddCodec(new string[] { "ogg" }, typeof(VorbisWaveReader));

            // Load external NAudio codecs
            foreach (ICodec c in GetPlugins<ICodec>(Instance.ExePath))
            {
                if (!c.WaveStreamType.IsSubclassOf(typeof(WaveStream)))
                    throw new InvalidOperationException("A plugin tried to register an NAudio codec which doesn't derive from WaveStream!");
                var e = from x in c.Extensions
                        select x.ToLower();
                NAudioPlayer.AddCodec(e, c.WaveStreamType);
            }

            // Register the NAudioPlayer
            PlaybackManager.Instance.RegisterAudioPlayer(NAudioPlayer.GetCodecs(), new NAudioPlayer());

            // Register external AudioPlayers
            foreach (IAudioPlayer a in GetPlugins<IAudioPlayer>(Instance.ExePath))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
        }
    }
}
