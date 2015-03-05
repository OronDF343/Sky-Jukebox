using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkyJukebox.Api.Playback;

namespace SkyJukebox.Core.Playback
{
    public class EqualizerBand : IEqualizerBand
    {
        private float _frequency;
        public float Frequency { get { return _frequency; } set { _frequency = value; OnPropertyChanged(); } }
        private float _gain;
        public float Gain { get { return _gain; } set { _gain = value; OnPropertyChanged(); } }
        private float _bandwidth;
        public float Bandwidth { get { return _bandwidth; } set { _bandwidth = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}