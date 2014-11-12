using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class Unknown : Metadata
    {
        protected byte[] Data;

        public Unknown(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            if (length > 0)
            {
                Data = new byte[length];
                inputStream.ReadByteBlockAlignedNoCRC(Data, length);
            }
        }
    }
}