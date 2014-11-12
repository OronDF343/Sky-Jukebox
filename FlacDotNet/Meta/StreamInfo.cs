using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    public class StreamInfo : Metadata
    {
        private const int STREAMINFO_MIN_BLOCK_SIZE_LEN = 16; // bits
        private const int STREAMINFO_MAX_BLOCK_SIZE_LEN = 16; // bits
        private const int STREAMINFO_MIN_FRAME_SIZE_LEN = 24; // bits
        private const int STREAMINFO_MAX_FRAME_SIZE_LEN = 24; // bits
        private const int STREAMINFO_SAMPLE_RATE_LEN = 20; // bits
        private const int STREAMINFO_CHANNELS_LEN = 3; // bits
        private const int STREAMINFO_BITS_PER_SAMPLE_LEN = 5; // bits
        private const int STREAMINFO_TOTAL_SAMPLES_LEN = 36; // bits
        private const int STREAMINFO_MD5SUM_LEN = 128; // bits

        private readonly byte[] md5sum = new byte[16];

        public StreamInfo(bool isLast)
            : base(isLast)
        {
        }

        public StreamInfo(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            int usedBits = 0;

            MinBlockSize = inputStream.ReadRawUInt(STREAMINFO_MIN_BLOCK_SIZE_LEN);
            usedBits += STREAMINFO_MIN_BLOCK_SIZE_LEN;

            MaxBlockSize = inputStream.ReadRawUInt(STREAMINFO_MAX_BLOCK_SIZE_LEN);
            usedBits += STREAMINFO_MAX_BLOCK_SIZE_LEN;

            MinFrameSize = inputStream.ReadRawUInt(STREAMINFO_MIN_FRAME_SIZE_LEN);
            usedBits += STREAMINFO_MIN_FRAME_SIZE_LEN;

            MaxFrameSize = inputStream.ReadRawUInt(STREAMINFO_MAX_FRAME_SIZE_LEN);
            usedBits += STREAMINFO_MAX_FRAME_SIZE_LEN;

            SampleRate = inputStream.ReadRawUInt(STREAMINFO_SAMPLE_RATE_LEN);
            usedBits += STREAMINFO_SAMPLE_RATE_LEN;

            Channels = inputStream.ReadRawUInt(STREAMINFO_CHANNELS_LEN) + 1;
            usedBits += STREAMINFO_CHANNELS_LEN;

            BitsPerSample = inputStream.ReadRawUInt(STREAMINFO_BITS_PER_SAMPLE_LEN) + 1;
            usedBits += STREAMINFO_BITS_PER_SAMPLE_LEN;

            TotalSamples = inputStream.ReadRawULong(STREAMINFO_TOTAL_SAMPLES_LEN);
            usedBits += STREAMINFO_TOTAL_SAMPLES_LEN;

            inputStream.ReadByteBlockAlignedNoCRC(md5sum, STREAMINFO_MD5SUM_LEN/8);
            usedBits += 16*8;

            // skip the rest of the block
            length -= (usedBits/8);
            inputStream.ReadByteBlockAlignedNoCRC(null, length);
        }

        public int MinBlockSize { get; private set; }
        public int MaxBlockSize { get; private set; }
        public int MinFrameSize { get; private set; }
        public int MaxFrameSize { get; private set; }
        public int SampleRate { get; private set; }
        public int Channels { get; private set; }
        public int BitsPerSample { get; private set; }
        public long TotalSamples { get; private set; }
    }
}