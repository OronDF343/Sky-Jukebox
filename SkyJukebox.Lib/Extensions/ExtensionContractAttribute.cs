using System;

namespace SkyJukebox.Lib.Extensions
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ExtensionContractAttribute : Attribute
    {
        public string Version { get; set; }
        public string MinTargetVersion { get; set; }

        public ExtensionContractAttribute(string version, string minTargetVersion)
        {
            Version = version;
            MinTargetVersion = minTargetVersion;
        }
    }
}
