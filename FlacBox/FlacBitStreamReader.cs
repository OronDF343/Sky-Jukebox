﻿using System.Diagnostics;
using System.IO;

namespace FlacBox
{
    /// <summary>
    /// Reads bit data such as unary, rice, unsigned and signed number. 
    /// Calculates CRC16 of the data.
    /// </summary>
    sealed class FlacBitStreamReader
    {
        Stream baseStream;
        ushort crc16;
        uint data = 0;
        int bits = 0;

        public Stream BaseStream
        {
            get { return baseStream; }
        }

        internal FlacBitStreamReader(Stream stream, ushort crc16Seed)
        {
            baseStream = stream;
            crc16 = crc16Seed;
        }

        private int ReadByte()
        {
            var i = BaseStream.ReadByte();
            if (i < 0) throw new FlacException("Unexpected end of stream: bit reader needs data");
            crc16 = CrcUtils.Crc16(crc16, (byte)i);
            return i;
        }

        public uint ReadBits(int n)
        {
            Debug.Assert(n > 0);

            if (n > 16)
            {
                var hi = ReadBits(16);
                var low = ReadBits(n - 16);
                return (hi << (n - 16)) | low;
            }

            while (bits < n)
            {
                data = (uint)(data << 8) | (uint)ReadByte();
                bits += 8;
            }
            var result = data >> (bits - n);
            bits -= n;
            data &= (uint)(1 << bits) - 1;
            return result;
        }

        public int ReadSignedBits(int n)
        {
            var signMask = 1 << (n - 1);
            var result = ReadBits(n);
            if ((result & signMask) == 0)
                return (int)result;
            else
                return unchecked((int)result | (0 - signMask));
        }

        public uint ReadUnary()
        {
            uint k = 0;
            if (data == 0 && bits > 0)
            {
                do
                {
                    k += (uint)bits;
                    data = (uint)ReadByte();
                    bits = 8;
                } while (data == 0);
            }
            
            var val = ReadBits(1);
            while (val == 0)
            {
                k++;
                val = ReadBits(1);
            }
            return k;
        }

        public int ReadRice(int riceParameter)
        {
            var k = ReadUnary();

            uint m;
            if (riceParameter == 0)
                m = 0;
            else
                m = ReadBits(riceParameter);

            var value = k << riceParameter | m;
            if ((value & 1) == 0)
                return (int)(value >> 1);
            else
                return (int)~(value >> 1);
        }

        public ushort Complete()
        {
            if (data != 0) throw new FlacException("Zeros expected for alignment padding");
            data = 0;
            bits = 0;

            baseStream = null;

            return crc16;
        }
    }
}