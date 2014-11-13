using System;
using System.Collections.Generic;

namespace SkyJukebox.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        // Basic info
        /// <summary>
        /// Gets or sets the unique ID of this plugin.
        /// </summary>
        public string PluginId { get; set; }
        /// <summary>
        /// Gets or sets the human-readable short description of this plugin.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the version of this plugin.
        /// </summary>
        public Version PluginVersion { get; set; }

        // API requirements
        /// <summary>
        /// Gets or sets the minimum API version supported by this plugin.
        /// </summary>
        public Version ApiMinimumVersion { get; set; }
        /// <summary>
        /// Gets or sets the API version that this plugin targets.
        /// </summary>
        public Version ApiTargetVersion { get; set; }
        /// <summary>
        /// Gets or sets a boolean value indicating whether to require the exact API version as specified by ApiTargetVersion.
        /// </summary>
        public bool RequireExactVersion { get; set; }

        // Other requirements
        /// <summary>
        /// Gets or sets a list of required plugin dependencies.
        /// </summary>
        public IEnumerable<Dependency> Dependencies { get; set; } 
    }

    public struct Dependency
    {
        public string PluginId;
        public Version MinimumVersion;
        public bool RequireExactVersion;
    }
}
