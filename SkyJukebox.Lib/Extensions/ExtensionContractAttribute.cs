using System;

namespace SkyJukebox.Lib.Extensions
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ExtensionContractAttribute : Attribute
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public ContractVersionPolicies ContractVersionPolicy { get; set; }

        public ExtensionContractAttribute(string id, string version, ContractVersionPolicies policy = ContractVersionPolicies.Default)
        {
            Id = id;
            Version = version;
            ContractVersionPolicy = policy;
        }
    }

    // TODO: Presets
    /// <summary>
    /// Determines how to act when the Extension's target contract version is different from the current contract version.
    /// </summary>
    [Flags]
    public enum ContractVersionPolicies
    {
        /// <summary>
        /// Warns the user if the major version number is different. This flag is always set!
        /// </summary>
        MajorWarning = 0x00,
        /// <summary>
        /// Must have same major version number.
        /// </summary>
        SameMajor = 0x01,
        /// <summary>
        /// Must have same minor version number. Implies MinorWarning.
        /// </summary>
        SameMinor = 0x22,
        /// <summary>
        /// Must have same revision number. Implies RevisionWarning.
        /// </summary>
        SameRevision = 0x44,
        /// <summary>
        /// Must have same build number. Implies BuildWarning.
        /// </summary>
        SameBuild = 0x88,
        /// <summary>
        /// Allow loading an extension which targets a newer contract version, in the same way an old one would be allowed.
        /// </summary>
        AllowNewer = 0x10,
        /// <summary>
        /// Warn the user if the minor version number is different.
        /// </summary>
        MinorWarning = 0x20,
        /// <summary>
        /// Warn the user if the revision number is different.
        /// </summary>
        RevisionWarning = 0x40,
        /// <summary>
        /// Warn the user if the build number is different.
        /// </summary>
        BuildWarning = 0x80, 

        /// <summary>
        /// Require same major and minor versions, can't be newer version, don't warn for older revision and build numbers.
        /// </summary>
        Default = SameMajor | SameMinor,
    }
}
