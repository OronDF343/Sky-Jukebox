using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Api
{
    [ExtensionContract("Plugin", "1.0.0.0")]
    public interface IPlugin
    {
        // display name
        string Name { get; }
        // display description
        string Description { get; }
        // plugin author
        string Author { get; }
        // link to homepage
        string Url { get; }
            
        // load plugin, return plugin icon
        // Tip: use the IPluginAccess to create the icon
        // File Icon example: IPluginAccess.CreateFileIcon(@"icon.png");
        // WPF Uri example: IPluginAccess.CreateEmbeddedIcon("pack://application:,,,/icon.png");
        // The icon will be automatically registered in the IconManager with the PluginId!!!
        IIcon Load(IPluginAccess contract);

        void ShowGui();

        // when the application is shut down
        void Unload();
    }
}
