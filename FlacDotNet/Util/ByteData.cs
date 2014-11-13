namespace FlacDotNet.Util
{
    public class ByteData
    {
        private const int DefaultBufferSize = 256;

        public ByteData(int maxSpace)
        {
            if (maxSpace <= 0) maxSpace = DefaultBufferSize;
            Data = new byte[maxSpace];
            Length = 0;
        }

        /** The byte array where data is stored. */
        public byte[] Data { get; internal set; }

        /** The number of bytes stored in the array. */
        public int Length { get; internal set; }

        /**
         * The default constructor.
         * @param maxSpace  The maximum space in the internal byte array.
         */


        /**
         * Append byte to storage.
         * @param b byte to extend
         */

        public void Append(byte b)
        {
            Data[Length++] = b;
        }


        public byte GetData(int idx)
        {
            return Data[idx];
        }

        public void SetLength(int len)
        {
            if (len > Data.Length)
            {
                len = Data.Length;
            }
            Length = len;
        }
    }
}