using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkyJukebox.Lib.Extensions
{
    public static class AssemblyLoader
    {
        private static IEnumerable<Type> GetExtensionTypes<TInterface>(string path)
        {
            if (!typeof(TInterface).IsInterface) return null;
            var pa = Assembly.GetCallingAssembly().GetName().ProcessorArchitecture;
            return from dllFile in Directory.GetFiles(path, "*.dll")
                   // first make sure that the dll has a compatible processor architecture
                   let n = AssemblyName.GetAssemblyName(dllFile)
                   where n.ProcessorArchitecture == pa || n.ProcessorArchitecture == ProcessorArchitecture.MSIL
                   // now load it and find the type
                   let a = Assembly.LoadFrom(dllFile)
                   where a != null
                   from t in a.GetTypes()
                   let pluginType = typeof(TInterface)
                   where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                   select t;
        }

        public static IEnumerable<TInterface> GetExtensions<TInterface>(string path)
        {
            if (!typeof(TInterface).IsInterface) return null;
            return from t in GetExtensionTypes<TInterface>(path)
                   select (TInterface)Activator.CreateInstance(t);
        }

        public static IEnumerable<ExtensionInfo<TInterface, TAttribute>> GetExtensions<TInterface, TAttribute>(string path) where TAttribute : Attribute
        {
            if (!typeof(TInterface).IsInterface) return null;
            return from t in GetExtensionTypes<TInterface>(path)
                   // make sure it has the attribute
                   let attr = t.GetCustomAttribute<TAttribute>()
                   where attr != null
                   // we found it, create an instance
                   let obj = (TInterface)Activator.CreateInstance(t)
                   select new ExtensionInfo<TInterface, TAttribute>(obj, attr);
        }
    }
}