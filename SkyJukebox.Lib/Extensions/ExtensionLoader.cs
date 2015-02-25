using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace SkyJukebox.Lib.Extensions
{
    public static class ExtensionLoader
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

        private static IEnumerable<Type> TryGetTypes(this Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(ex.LoaderExceptions[0]);
                return new Type[0];
            }
        }

        private static bool EvaluateProcessorArchitecture(ProcessorArchitecture current, ProcessorArchitecture extension)
        {
            // TODO: Error messages
            return current == extension || extension == ProcessorArchitecture.MSIL;
        }

        private static IEnumerable<Type> GetExtensionTypes<TInterface>(string path, string callingAssembly)
        {
            if (!typeof(TInterface).IsInterface) return null;
            var pa = Assembly.GetCallingAssembly().GetName().ProcessorArchitecture;
            return from dllFile in Directory.GetFiles(path, "*.dll")
                   where dllFile != callingAssembly
                   // first make sure that the dll has a compatible processor architecture
                   let n = AssemblyName.GetAssemblyName(dllFile)
                   where EvaluateProcessorArchitecture(pa, n.ProcessorArchitecture)
                   // now load it and find the type
                   let a = Assembly.LoadFrom(dllFile)
                   where a != null
                   from t in a.TryGetTypes()
                   let pluginType = typeof(TInterface)
                   where !t.IsInterface && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null
                   select t;
        }

        public static IEnumerable<TInterface> GetExtensions<TInterface>(string path)
        {
            if (!typeof(TInterface).IsInterface) return null;
            return from t in GetExtensionTypes<TInterface>(path, Assembly.GetCallingAssembly().Location)
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select (TInterface)obj;
        }

        public static IEnumerable<ExtensionInfo<TInterface>> GetCompatibleExtensions<TInterface>(string path)
        {
            var itype = typeof(TInterface);
            if (!itype.IsInterface) return null;
            var cattr = itype.GetCustomAttribute<ExtensionContractAttribute>();
            if (cattr == null) return null;
            return from t in GetExtensionTypes<TInterface>(path, Assembly.GetCallingAssembly().Location)
                   // make sure it has the attribute
                   let attr = t.GetCustomAttribute<ExtensionAttribute>()
                   where attr != null
                   // check version
                   let ver = new Version(attr.TargetContractVersion)
                   let cver = new Version(cattr.Version)
                   let mver = new Version(cattr.MinTargetVersion)
                   where ver >= mver && ver <= cver
                   // we found it, create an instance
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select new ExtensionInfo<TInterface>((TInterface)obj, attr);
        }
    }
}