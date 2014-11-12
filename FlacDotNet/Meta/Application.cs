using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class Application : Metadata
    {
        private const int APPLICATION_ID_LEN = 32; // bits

        private readonly byte[] _data;
        private readonly byte[] _id = new byte[4];

        public Application(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            inputStream.ReadByteBlockAlignedNoCRC(_id, APPLICATION_ID_LEN/8);
            length -= APPLICATION_ID_LEN/8;

            if (length > 0)
            {
                _data = new byte[length];
                inputStream.ReadByteBlockAlignedNoCRC(_data, length);
            }
        }
    }
}