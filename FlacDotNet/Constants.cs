namespace FlacDotNet
{
    internal static class Constants
    {
        /** The maximum number of audio channels. */
        public const int MaxChannels = 8;

        /** The maximum frame block size. */
        public const int MaxBlockSize = 65535;

        /** The maximum Rice partition order permitted by the format. */
        public const int MaxRicePartitionOrder = 15;

        /** independent channels. */
        public const int ChannelAssignmentIndependent = 0;
        /** left+side stereo. */
        public const int ChannelAssignmentLeftSide = 1;
        /** right+side stereo. */
        public const int ChannelAssignmentRightSide = 2;
        /** mid+side stereo. */
        public const int ChannelAssignmentMidSide = 3;

        /** FLAC Stream Sync string. */
        public static readonly byte[] StreamSyncString = {(byte) 'f', (byte) 'L', (byte) 'a', (byte) 'C'};
    }
}