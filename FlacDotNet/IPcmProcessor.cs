using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet
{
    public interface IPcmProcessor
    {
        void ProcessStreamInfo(ref StreamInfo streamInfo);

        /**
         * Called when each data frame is decompressed.
         * @param pcm The decompressed PCM data
         */
        void ProcessPcm(ByteData pcm);
    }
}