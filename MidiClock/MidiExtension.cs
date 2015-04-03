using SkyJukebox.Api;
using SkyJukebox.Lib.Extensions;

namespace MidiClock
{
    [Extension("MidiClock", "0.1.0.0", "1.0.0.0")]
    public class MidiExtension : IExtension
    {
        public string Name { get { return "MidiClock"; } }
        public string Description { get { return "MIDI playback support."; } }
        public string Author { get { return "OronDF343; includes MidiUtils by Tomona Nanase"; } }
        public string Url { get { return "https://github.com/nanase/MidiUtils" + " " + "https://github.com/OronDF343/Sky-Jukebox"; } }

        private IExtensionAccess _access;
        private MidiPlayer _midiPlayer;

        public void Init(IExtensionAccess contract)
        {
            _access = contract;
            _midiPlayer = new MidiPlayer();
            _access.PlaybackManagerInstance.RegisterAudioPlayer(_midiPlayer.Extensions, _midiPlayer);
        }

        private MidiMonitorWindow _window;

        public void InitGui()
        {
            _access.AddPluginButton("midiBtn", "info32", () =>
            {
                if (_window == null) _window = new MidiMonitorWindow(_midiPlayer);
                _window.Show();
            }, "MIDI Monitor");
        }

        public void Unload()
        {
            try { _window.Close(); }
            catch { }
        }
    }
}
