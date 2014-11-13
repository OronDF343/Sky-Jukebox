using System;
using System.Collections.Generic;

namespace SkyJukebox.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExtensionAttribute : Attribute
    {
        // Basic info
        /// <summary>
        /// Gets or sets the unique ID of this Extension.
        /// </summary>
        public string ExtensionId { get; set; }
        /// <summary>
        /// Gets or sets the human-readable short description of this Extension.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the version of this Extension.
        /// </summary>
        public Version ExtensionVersion { get; set; }

        // API requirements
        /// <summary>
        /// Gets or sets the minimum API version supported by this Extension.
        /// </summary>
        public Version ApiMinimumVersion { get; set; }
        /// <summary>
        /// Gets or sets the API version that this Extension targets.
        /// </summary>
        public Version ApiTargetVersion { get; set; }
        /// <summary>
        /// Gets or sets a boolean value indicating whether to require the exact API version as specified by ApiTargetVersion.
        /// </summary>
        public bool RequireExactVersion { get; set; }
    }
}
