using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkyJukebox.PluginAPI
{
    public static class PluginInteraction
    {
        public static List<IPlugin> GetPlugins(string path)
        {
            // If this works, then this is some of my favorite code ^_^
            return (from dllFile in Directory.GetFiles(path, "*.dll")
                    let a = Assembly.Load(AssemblyName.GetAssemblyName(dllFile))
                    where a != null
                    from t in a.GetTypes()
                    let pluginType = typeof(IPlugin)
                    where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                    select (IPlugin)Activator.CreateInstance(t)).ToList();
        }
        public static List<ICodec> GetCodecs(string path)
        {
            // If this works, then this is some of my favorite code ^_^
            return (from dllFile in Directory.GetFiles(path, "*.dll")
                    let a = Assembly.Load(AssemblyName.GetAssemblyName(dllFile))
                    where a != null
                    from t in a.GetTypes()
                    let pluginType = typeof(ICodec)
                    where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                    select (ICodec)Activator.CreateInstance(t)).ToList();
        }
    }
}
