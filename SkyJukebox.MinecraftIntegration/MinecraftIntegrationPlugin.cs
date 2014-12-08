using System.Threading;
using SkyJukebox.Api;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.MinecraftIntegration
{
    [Extension("Minecraft Integration", "1.0.0.0", "1.0.0.0")]
    public class MinecraftIntegrationPlugin : IPlugin
    {
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
            _myPipe = new PipeServer { PlaybackManager = _access.GetPlaybackManager() };
            //_th = new Thread(() => _myPipe.Run());
            //_th.Start();
            return _access.CreateFileIcon(Utils.GetExePath() + "mc.png");
        }

        public void ShowGui()
        {
            
        }

        public void Unload()
        {
            //_th.Abort();
        }
    }
}
