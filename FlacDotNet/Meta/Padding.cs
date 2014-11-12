using System;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class Padding : Metadata
    {
        private readonly int _length;

        /**
         * The constructor.
         * @param is                The InputBitStream
         * @param length            Length of the record
         * @param isLast            True if this is the last Metadata block in the chain
         * @throws IOException      Thrown if error reading from InputBitStream
         */

        public Padding(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            _length = length;
            inputStream.ReadByteBlockAlignedNoCRC(null, length);
        }

        /**
         * Convert to string.
         * @see java.lang.Object#toString()
         */

        public override String ToString()
        {
            return "Padding (Length=" + _length + ")";
        }
    }
}