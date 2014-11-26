using System;

namespace SkyJukebox.Lib.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExtensionAttribute : Attribute
    {
        // Basic info
        /// <summary>
        /// Gets or sets the unique ID of this Extension.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the human-readable short description of this Extension.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the version of this Extension.
        /// </summary>
        public string Version { get; set; }

        // Contract requirements
        /// <summary>
        /// Gets or sets the name of the Extension Contract.
        /// </summary>
        public string ContractId { get; set; }
        /// <summary>
        /// Gets or sets the minimum Contract version supported by this Extension.
        /// </summary>
        public string ContractMinimumVersion { get; set; }

        public ExtensionAttribute(string id, string version, string contractId, string contractMinVer)
        {
            Id = id;
            Version = version;
            ContractId = contractId;
            ContractMinimumVersion = contractMinVer;
        }
    }
}
