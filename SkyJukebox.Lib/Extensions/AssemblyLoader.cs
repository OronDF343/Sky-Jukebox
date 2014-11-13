using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace SkyJukebox.Lib.Extensions
{
    public static class AssemblyLoader
    {
        private static object TryCreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to create an instance of the plugin type {0}:\n{1}", type.Name, ex.Message), "Error loading plugin");
                return null;
            }
        }

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
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select (TInterface)obj;
        }

        public static IEnumerable<ExtensionInfo<TInterface, TAttribute>> GetExtensions<TInterface, TAttribute>(string path) where TAttribute : Attribute
        {
            if (!typeof(TInterface).IsInterface) return null;
            return from t in GetExtensionTypes<TInterface>(path)
                   // make sure it has the attribute
                   let attr = t.GetCustomAttribute<TAttribute>()
                   where attr != null
                   // we found it, create an instance
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select new ExtensionInfo<TInterface, TAttribute>((TInterface)obj, attr);
        }
    }
}