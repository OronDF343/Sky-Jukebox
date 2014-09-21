using System;
using System.Collections.Generic;

namespace SkyJukebox.PluginAPI
{
    public interface IPlugin
    {
        // unique internal name for the plugin
        string PluginID { get; }
        // displayed name
        string Name { get; }
        // displayed description
        string Description { get; }
        // plugin author
        string Author { get; }
        // link to download page
        string URL { get; }
        // plugin version
        Version Version { get; }

        // load plugin, return error code. 0 = ok
        int Load();
    }
}
