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
            // TODO: Error message
            return current == extension || extension == ProcessorArchitecture.MSIL;
        }

        private static IEnumerable<Type> GetExtensionTypes<TInterface>(string path)
        {
            if (!typeof(TInterface).IsInterface) return null;
            var pa = Assembly.GetCallingAssembly().GetName().ProcessorArchitecture;
            return from dllFile in Directory.GetFiles(path, "*.dll")
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
            return from t in GetExtensionTypes<TInterface>(path)
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select (TInterface)obj;
        }

        private static bool EvaluateVersion(string type, string id, Version contract, Version extension, ContractVersionPolicies policy)
        {
            if (!policy.HasFlag(ContractVersionPolicies.AllowNewer) && extension > contract)
            {
                MessageBox.Show(string.Format("Can't load the {0} {1}: Required contract version is too new!", type, id),
                                "Extension Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if ((policy.HasFlag(ContractVersionPolicies.SameMajor) && extension.Major != contract.Major)
                || (policy.HasFlag(ContractVersionPolicies.SameMinor) && extension.Minor != contract.Minor)
                || (policy.HasFlag(ContractVersionPolicies.SameRevision) && extension.Revision != contract.Revision)
                || (policy.HasFlag(ContractVersionPolicies.SameBuild) && extension.Build != contract.Build))
            {
                MessageBox.Show(string.Format("Can't load the {0} {1}: Required contract version is too {2}!", type, id, extension > contract ? "new" : "old"),
                                "Extension Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if ((policy.HasFlag(ContractVersionPolicies.MajorWarning) && extension.Major != contract.Major)
                || (policy.HasFlag(ContractVersionPolicies.MinorWarning) && extension.Minor != contract.Minor)
                || (policy.HasFlag(ContractVersionPolicies.RevisionWarning) && extension.Revision != contract.Revision)
                || (policy.HasFlag(ContractVersionPolicies.BuildWarning) && extension.Build != contract.Build))
            {
                var result = MessageBox.Show(string.Format("Warning: The {0} {1} requires {2} version of the contract, load it anyway?", type, id, extension > contract ? "a newer" : "an older"),
                                "Extension Loader", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                return result == MessageBoxResult.Yes;
            }
            return true;
        }

        public static IEnumerable<ExtensionInfo<TInterface>> GetCompatibleExtensions<TInterface>(string path)
        {
            var itype = typeof(TInterface);
            if (!itype.IsInterface) return null;
            var cattr = itype.GetCustomAttribute<ExtensionContractAttribute>();
            if (cattr == null) return null;
            return from t in GetExtensionTypes<TInterface>(path)
                   // make sure it has the attribute
                   let attr = t.GetCustomAttribute<ExtensionAttribute>()
                   where attr != null
                   // check version
                   let ver = new Version(attr.ContractMinimumVersion)
                   let cver = new Version(cattr.Version)
                   where EvaluateVersion(cattr.Id, attr.Id, cver, ver, cattr.ContractVersionPolicy)
                   // we found it, create an instance
                   let obj = TryCreateInstance(t)
                   where obj != null
                   select new ExtensionInfo<TInterface>((TInterface)obj, attr);
        }
    }
}