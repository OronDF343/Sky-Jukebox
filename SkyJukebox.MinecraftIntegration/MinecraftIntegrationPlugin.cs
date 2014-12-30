using System.Threading;
using System.Windows;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;
using SkyJukebox.Lib.Icons;

namespace SkyJukebox.MinecraftIntegration
{
    [Extension("MinecraftIntegration", "1.0.0.0", "1.0.0.0")]
    public class MinecraftIntegrationPlugin : IPlugin
    {
        public string Name
        {
            get { return "Minecraft Integration"; }
        }

        public string Description
        {
            get { return "Allows Sky Jukebox to be controlled externally from Minecraft."; }
        }

        public string Author
        {
            get { return "OronDF343"; }
        }

        public string Url
        {
            get { return "github.com/OronDF343/Sky-Jukebox"; }
        }

        private IPluginAccess _access;
        private PipeServer _myPipe;
        private Thread _th;
        public IIcon Load(IPluginAccess contract)
        {
            _access = contract;
            _myPipe = new PipeServer { PlaybackManager = _access.PlaybackManagerInstance };
            //_th = new Thread(() => _myPipe.Run());
            //_th.Start();
            return _access.CreateFileIcon(Utils.GetExePath() + "mc.png");
        }

        public void ShowGui()
        {
            MessageBox.Show("Not yet implemented", "MCI", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Unload()
        {
            //_th.Abort();
        }
    }
}
