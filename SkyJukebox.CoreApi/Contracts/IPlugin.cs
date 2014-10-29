using System;

namespace SkyJukebox.CoreApi.Contracts
{
    public interface IPlugin
    {
        // unique internal name for the plugin
        string PluginId { get; }
        // display name
        string Name { get; }
        // display description
        string Description { get; }
        // plugin author
        string Author { get; }
        // link to download page
        string Url { get; }
        // plugin version
        Version Version { get; }
        // plugin button icon
        string IconPath { get; }

        // load plugin, return error code. 0 = ok
        void Load(IPluginAccess contract);
    }
}
