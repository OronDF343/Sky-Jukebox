﻿using System;

namespace SkyJukebox.Api
{
    public interface IPlugin
    {
        // display description
        string Description { get; }
        // plugin author
        string Author { get; }
        // link to homepage
        string Url { get; }

        // load plugin, return plugin icon
        // Tip: use the IPluginAccess to create the icon
        // File Icon example (can be any format): IPluginAccess.CreateFileIcon(@"icon.png");
        // WPF Uri example (must be PNG as of now): IPluginAccess.CreateEmbeddedIcon("pack://application:,,,/icon.png");
        // The icon will be automatically registered in the IconManager with the PluginId!!!
        IIcon Load(IPluginAccess contract);
    }
}
