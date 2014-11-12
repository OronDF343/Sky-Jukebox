using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class CueIndex
    {
        private const int CUESHEET_INDEX_OFFSET_LEN = 64; // bits
        private const int CUESHEET_INDEX_NUMBER_LEN = 8; // bits
        private const int CUESHEET_INDEX_RESERVED_LEN = 3*8; // bits

        internal byte Number; // The index point number.
        internal long Offset; // Offset in samples, relative to the track offset, of the index point.

        public CueIndex(BitInputStream inputStream)
        {
            Offset = inputStream.ReadRawULong(CUESHEET_INDEX_OFFSET_LEN);
            Number = (byte) inputStream.ReadRawUInt(CUESHEET_INDEX_NUMBER_LEN);
            inputStream.SkipBitsNoCRC(CUESHEET_INDEX_RESERVED_LEN);
        }
    }
}