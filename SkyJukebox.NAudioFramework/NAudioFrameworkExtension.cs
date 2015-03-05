using System.Linq;
using SkyJukebox.Api;
using SkyJukebox.Lib.Extensions;
using SkyJukebox.Lib.Icons;

namespace SkyJukebox.NAudioFramework
{
    [Extension("NAudioFramework", "1.0.0.0", "1.0.0.0")]
    public class NAudioFrameworkExtension : IExtension
    {
        public string Name { get { return "NAudio Playback Framework"; } }
        public string Description { get { return "The main playback engine of Sky Jukebox."; } }
        public string Author { get { return "OronDF343"; } }
        public string Url { get { return "https://github.com/OronDF343/Sky-Jukebox"; } }

        internal IExtensionAccess Access;
        internal NAudioPlayer Np;
        private EqualizerWindow _window;

        public void Init(IExtensionAccess contract)
        {
            Access = contract;
            Np = new NAudioPlayer();
            Np.Init();
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 31, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 62, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 125, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 250, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 500, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 1000, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 2000, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 4000, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 8000, Gain = 0 });
            Np.EqualizerBands.Add(new TempEqBand { Bandwidth = 1f, Frequency = 16000, Gain = 0 });
            Access.PlaybackManagerInstance.RegisterAudioPlayer(from ec in Np.GetCodecInfo().Values from e in ec select e, Np);
        }

        public void InitGui()
        {
            _window = new EqualizerWindow(this);
            Access.IconManagerInstance.Add("equalizer32", new EmbeddedPngIcon("pack://application:,,,/IconFiles/iconmonstr-equalizer-icon-32.png"));
            Access.AddPluginButton("NAudioFramework:WIP_EQ", "equalizer32", () => _window.Show(), "Equalizer (WIP)");
        }

        public void Unload()
        {
            _window.CloseFinal();
        }
    }
}
