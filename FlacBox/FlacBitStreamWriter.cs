using System.Diagnostics;
using System.IO;

namespace FlacBox
{
    /// <summary>
    /// Writes bit data such as unary, rice, unsigned and signed number. 
    /// Calculates CRC16 of the data.
    /// </summary>
    sealed class FlacBitStreamWriter
    {
        uint buffer = 0;
        int bufferSize = 0;

        Stream baseStream;
        ushort crc16;
        int count;

        internal FlacBitStreamWriter(Stream stream, ushort crc16Seed)
        {
            baseStream = stream;
            crc16 = crc16Seed;
            count = 0;
        }

        private void WriteByte(byte b)
        {
            baseStream.WriteByte(b);
            crc16 = CrcUtils.Crc16(crc16, b);
            ++count;
        }

        internal void Complete(out ushort crc16, out int count)
        {
            if (bufferSize > 0)
            {
                WriteByte((byte)buffer);
                buffer = 0;
                bufferSize = 0;
            }            

            baseStream = null;

            crc16 = this.crc16;
            count = this.count;
        }

        internal void WriteSigned(int data, int bits)
        {
            WriteUnsigned((uint)data, bits);
        }

        internal void WriteUnsigned(uint data, int bits)
        {
            var mask = (1U << bits) - 1;
            data &= mask;
            if (bufferSize + bits < 8)
            {
                buffer |= data << (8 - bits - bufferSize);
                bufferSize += bits;
            }
            else
            {
                var tail = 8 - bufferSize;
                buffer |= data >> (bits - tail);
                WriteByte((byte)buffer);
                bits -= tail;
                while (bits >= 8)
                {
                    bits -= 8;
                    WriteByte((byte)(data >> bits));
                }
                buffer = (byte)(data << (8 - bits));
                bufferSize = bits;
            }
        }

        internal void WriteRice(int number, int riceParameter)
        {
            uint i;
            if (number < 0)
                i = ((uint)(~number << 1)) | 1U;
            else
                i = (uint)(number << 1);

            WriteUnary(i >> riceParameter);
            if (riceParameter > 0)
            {
                WriteUnsigned(i, riceParameter);
            }
        }

        internal void WriteUnary(uint number)
        {
            var bufferTail = 8 - bufferSize;
            if (bufferTail <= number)
            {
                WriteUnsigned(0, bufferTail);
                Debug.Assert(bufferSize == 0);

                number -= (uint)bufferTail;
                while (number >= 8)
                {
                    WriteByte(0);
                    number -= 8;
                }
            }
            WriteUnsigned(1U, 1 + (int)number);
        }
    }
}
