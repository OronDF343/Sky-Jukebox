using System.ComponentModel;

namespace SkyJukebox.Api.Playback
{
    public interface IEqualizerBand : INotifyPropertyChanged
    {
        float Frequency { get; set; }
        float Gain { get; set; }
        float Bandwidth { get; set; }
    }
}
