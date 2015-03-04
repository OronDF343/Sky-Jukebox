namespace SkyJukebox.Api.Playback
{
    public interface IEqualizerBand
    {
        float Frequency { get; set; }
        float Gain { get; set; }
        float Bandwidth { get; set; }
    }
}
