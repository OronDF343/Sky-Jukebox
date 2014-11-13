using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.Lib
{
    public static class Utils
    {
        public static IEnumerable<T> GetPlugins<T>(string path)
        {
            // If this works, then this is some of my favorite code ^_^
            if (!typeof(T).IsInterface) return null;
            var pa = Assembly.GetCallingAssembly().GetName().ProcessorArchitecture;
            return from dllFile in Directory.GetFiles(path, "*.dll")
                   // first make sure that the dll has a compatible processor architecture
                   let n = AssemblyName.GetAssemblyName(dllFile)
                   where n.ProcessorArchitecture == pa || n.ProcessorArchitecture == ProcessorArchitecture.MSIL
                   // now load it and find the plugin type
                   let a = Assembly.LoadFrom(dllFile)
                   where a != null
                   from t in a.GetTypes()
                   let pluginType = typeof(T)
                   where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                   // we found it, create an instance
                   select (T)Activator.CreateInstance(t);
        }

        public static string SubstringRange(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string GetExt(this string path)
        {
            return path.SubstringRange(path.LastIndexOf('.') + 1, path.Length).ToLowerInvariant();
        }
    }
}
