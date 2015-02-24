using System.Linq;
using SkyJukebox.Api;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.NAudioFramework
{
    [Extension("NAudioFramework", "1.0.0.0", "1.0.0.0")]
    public class NAudioFrameworkExtension : IExtension
    {
        public string Name { get { return "NAudio Playback Framework"; } }
        public string Description { get { return "The main playback engine of Sky Jukebox."; } }
        public string Author { get { return "OronDF343"; } }
        public string Url { get { return "https://github.com/OronDF343/Sky-Jukebox"; } }

        private IExtensionAccess _access;
        private NAudioPlayer _np;

        public void Init(IExtensionAccess contract)
        {
            _access = contract;
            _np = new NAudioPlayer();
            _np.Init();
            _access.PlaybackManagerInstance.RegisterAudioPlayer(from ec in _np.GetCodecInfo().Values from e in ec select e, _np);
        }

        public void InitGui()
        {
            // nothing
        }

        public void Unload()
        {
            // Disposal of AudioPlayers is handled by the PlaybackManager.
        }
    }
}
