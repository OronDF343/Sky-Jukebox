using NAudio.Wave;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public abstract class SampleStream : WaveStream, ISampleProvider
    {
        public abstract int Read(float[] buffer, int offset, int count);
    }
}
