namespace FlacDotNet
{
    internal class Constants
    {
        /** The maximum number of audio channels. */
        public const int MAX_CHANNELS = 8;

        /** The maximum frame block size. */
        public const int MAX_BLOCK_SIZE = 65535;

        /** The maximum Rice partition order permitted by the format. */
        public const int MAX_RICE_PARTITION_ORDER = 15;

        /** independent channels. */
        public const int CHANNEL_ASSIGNMENT_INDEPENDENT = 0;
        /** left+side stereo. */
        public const int CHANNEL_ASSIGNMENT_LEFT_SIDE = 1;
        /** right+side stereo. */
        public const int CHANNEL_ASSIGNMENT_RIGHT_SIDE = 2;
        /** mid+side stereo. */
        public const int CHANNEL_ASSIGNMENT_MID_SIDE = 3;

        /** FLAC Stream Sync string. */
        public static byte[] STREAM_SYNC_STRING = new[] {(byte) 'f', (byte) 'L', (byte) 'a', (byte) 'C'};
    }
}