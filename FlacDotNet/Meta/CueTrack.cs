using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class CueTrack
    {
        private const int CUESHEET_TRACK_OFFSET_LEN = 64; // bits
        private const int CUESHEET_TRACK_NUMBER_LEN = 8; // bits
        private const int CUESHEET_TRACK_ISRC_LEN = 12*8; // bits
        private const int CUESHEET_TRACK_TYPE_LEN = 1; // bit
        private const int CUESHEET_TRACK_PRE_EMPHASIS_LEN = 1; // bit
        private const int CUESHEET_TRACK_RESERVED_LEN = 6 + 13*8; // bits
        private const int CUESHEET_TRACK_NUM_INDICES_LEN = 8; // bits
        internal CueIndex[] Indices; // NULL if num_indices == 0, else pointer to array of index points.

        internal byte[] Isrc = new byte[13]; // Track ISRC.  This is a 12-digit alphanumeric code plus a trailing '\0'
        internal byte NumIndices; // The number of track index points.
        internal byte Number; // The track number.
        internal long Offset; // Track offset in samples, relative to the beginning of the FLAC audio stream.
        internal int PreEmphasis; // The pre-emphasis flag: 0 for no pre-emphasis, 1 for pre-emphasis.
        internal int Type; // The track type: 0 for audio, 1 for non-audio.

        public CueTrack(BitInputStream inputStream)
        {
            Offset = inputStream.ReadRawULong(CUESHEET_TRACK_OFFSET_LEN);
            Number = (byte) inputStream.ReadRawUInt(CUESHEET_TRACK_NUMBER_LEN);
            inputStream.ReadByteBlockAlignedNoCRC(Isrc, CUESHEET_TRACK_ISRC_LEN/8);
            Type = inputStream.ReadRawUInt(CUESHEET_TRACK_TYPE_LEN);
            PreEmphasis = inputStream.ReadRawUInt(CUESHEET_TRACK_PRE_EMPHASIS_LEN);
            inputStream.SkipBitsNoCRC(CUESHEET_TRACK_RESERVED_LEN);
            NumIndices = (byte) inputStream.ReadRawUInt(CUESHEET_TRACK_NUM_INDICES_LEN);
            if (NumIndices > 0)
            {
                Indices = new CueIndex[NumIndices];
                for (int j = 0; j < NumIndices; j++)
                {
                    Indices[j] = new CueIndex(inputStream);
                }
            }
        }
    }
}