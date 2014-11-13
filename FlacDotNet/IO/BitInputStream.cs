using System;
using System.IO;
using FlacDotNet.Util;

namespace FlacDotNet.IO
{
    public class BitInputStream
    {
        private const int BITS_PER_BLURB = 8;
        private const int BITS_PER_BLURB_LOG2 = 3;
        private const byte BLURB_TOP_BIT_ONE = (0x80);

        private const int BUFFER_CHUNK_SIZE = 1024;
        private readonly byte[] buffer = new byte[BUFFER_CHUNK_SIZE];
        private readonly Stream inStream;
        private long availBits;
        private int getBit;
        private int getByte;
        private int putByte;

        private int readCRC16;
        private long totalBitsRead;

        /**
         * The constructor.
         * @param is    The InputStream to read bits from
         */

        public BitInputStream(Stream inputStream)
        {
            inStream = inputStream;
        }

        private int ReadFromStream()
        {
            try
            {

            // first shift the unconsumed buffer data toward the front as much as possible
            if (getByte > 0 && putByte > getByte)
            {
                Array.Copy(buffer, getByte, buffer, 0, putByte - getByte);
            }
            putByte -= getByte;
            getByte = 0;

            // set the target for reading, taking into account blurb alignment
            // blurb == byte, so no gyrations necessary:
            int bytes = buffer.Length - putByte;

            // finally, read in some data
            bytes = inStream.Read(buffer, putByte, bytes);
            if (bytes <= 0) throw new EndOfStreamException();

            // now we have to handle partial blurb cases:
            // blurb == byte, so no gyrations necessary:
            putByte += bytes;
            availBits += bytes << 3;
            return bytes;
            }
            catch (ObjectDisposedException)
            {
                
            }
            return 0;
        }

        /**
         * Reset the bit stream.
         */

        public void Reset()
        {
            getByte = 0;
            getBit = 0;
            putByte = 0;
            availBits = 0;
        }

        /**
         * Reset the read CRC-16 value.
         * @param seed  The initial CRC-16 value
         */

        public void ResetReadCRC16(int seed)
        {
            readCRC16 = seed;
        }

        /**
         * return the read CRC-16 value.
         * @return  The read CRC-16 value
         */

        public int GetReadCRC16()
        {
            return readCRC16;
        }

        /**
         * Test if the Bit Stream consumed bits is byte aligned.
         * @return  True of bit stream consumed bits is byte aligned
         */

        public bool IsConsumedByteAligned()
        {
            return ((getBit & 7) == 0);
        }

        /**
         * return the number of bits to read to align the byte.
         * @return  The number of bits to align the byte
         */

        public int BitsLeftForByteAlignment()
        {
            return 8 - (getBit & 7);
        }

        /**
         * return the number of bytes left to read.
         * @return  The number of bytes left to read
         */

        public long GETInputBytesUnconsumed()
        {
            return availBits >> 3;
        }

        /**
         * skip over bits in bit stream without updating CRC.
         * @param bits  Number of bits to skip
         * @throws IOException  Thrown if error reading from input stream
         */

        public void SkipBitsNoCRC(int bits)
        {
            if (bits == 0) return;
            int bitsToAlign = getBit & 7;
            if (bitsToAlign != 0)
            {
                int bitsToTake = Math.Min(8 - bitsToAlign, bits);
                ReadRawUInt(bitsToTake);
                bits -= bitsToTake;
            }
            int bytesNeeded = bits/8;
            if (bytesNeeded > 0)
            {
                ReadByteBlockAlignedNoCRC(null, bytesNeeded);
                bits %= 8;
            }
            if (bits > 0)
            {
                ReadRawUInt(bits);
            }
        }

        /**
         * read a single bit.
         * @return  The bit
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadBit()
        {
            while (true)
            {
                if (availBits > 0)
                {
                    int val = ((buffer[getByte] & (0x80 >> getBit)) != 0) ? 1 : 0;
                    getBit++;
                    if (getBit == BITS_PER_BLURB)
                    {
                        readCRC16 = Crc16.Update(buffer[getByte], readCRC16);
                        getByte++;
                        getBit = 0;
                    }
                    availBits--;
                    totalBitsRead++;
                    return val;
                }
                else
                {
                    ReadFromStream();
                }
            }
        }

        /**
         * read a bit into an integer value.
         * The bits of the input integer are shifted left and the 
         * read bit is placed into bit 0.
         * @param val   The integer to shift and add read bit
         * @return      The updated integer value
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadBitToInt(int val)
        {
            while (true)
            {
                if (availBits > 0)
                {
                    val <<= 1;
                    val |= ((buffer[getByte] & (0x80 >> getBit)) != 0) ? 1 : 0;
                    getBit++;
                    if (getBit == BITS_PER_BLURB)
                    {
                        readCRC16 = Crc16.Update(buffer[getByte], readCRC16);
                        getByte++;
                        getBit = 0;
                    }
                    availBits--;
                    totalBitsRead++;
                    return val;
                }
                else
                {
                    ReadFromStream();
                }
            }
        }

        /**
         * peek at the next bit and add it to the input integer.
         * The bits of the input integer are shifted left and the 
         * read bit is placed into bit 0.
         * @param val   The input integer
         * @param bit   The bit to peek at
         * @return      The updated integer value
         * @throws IOException  Thrown if error reading input stream
         */

        public int PeekBitToInt(int val, int bit)
        {
            while (true)
            {
                if (bit < availBits)
                {
                    val <<= 1;
                    if ((getBit + bit) >= BITS_PER_BLURB)
                    {
                        bit = (getBit + bit)%BITS_PER_BLURB;
                        val |= ((buffer[getByte + 1] & (0x80 >> bit)) != 0) ? 1 : 0;
                    }
                    else
                    {
                        val |= ((buffer[getByte] & (0x80 >> (getBit + bit))) != 0) ? 1 : 0;
                    }
                    return val;
                }
                else
                {
                    ReadFromStream();
                }
            }
        }

        /**
         * read a bit into a long value.
         * The bits of the input long are shifted left and the 
         * read bit is placed into bit 0.
         * @param val   The long to shift and add read bit
         * @return      The updated long value
         * @throws IOException  Thrown if error reading input stream
         */

        public long ReadBitToLong(long val)
        {
            while (true)
            {
                if (availBits > 0)
                {
                    val <<= 1;
                    val |= ((buffer[getByte] & (0x80 >> getBit)) != 0) ? 1 : 0;
                    getBit++;
                    if (getBit == BITS_PER_BLURB)
                    {
                        readCRC16 = Crc16.Update(buffer[getByte], readCRC16);
                        getByte++;
                        getBit = 0;
                    }
                    availBits--;
                    totalBitsRead++;
                    return val;
                }
                else
                {
                    ReadFromStream();
                }
            }
        }

        /**
         * read bits into an unsigned integer.
         * @param bits  The number of bits to read
         * @return      The bits as an unsigned integer
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadRawUInt(int bits)
        {
            int val = 0;
            for (int i = 0; i < bits; i++)
            {
                val = ReadBitToInt(val);
            }
            return val;
        }

        /**
         * peek at bits into an unsigned integer without advancing the input stream.
         * @param bits  The number of bits to read
         * @return      The bits as an unsigned integer
         * @throws IOException  Thrown if error reading input stream
         */

        public int PeekRawUInt(int bits)
        {
            int val = 0;
            for (int i = 0; i < bits; i++)
            {
                val = PeekBitToInt(val, i);
            }
            return val;
        }

        /**
         * read bits into a signed integer.
         * @param bits  The number of bits to read
         * @return      The bits as a signed integer
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadRawInt(int bits)
        {
            if (bits == 0)
            {
                return 0;
            }
            int uval = 0;
            for (int i = 0; i < bits; i++)
            {
                uval = ReadBitToInt(uval);
            }

            // fix the sign
            int val;
            int bitsToleft = 32 - bits;
            if (bitsToleft != 0)
            {
                uval <<= bitsToleft;
                val = uval;
                val >>= bitsToleft;
            }
            else
            {
                val = uval;
            }
            return val;
        }

        /**
         * read bits into an unsigned long.
         * @param bits  The number of bits to read
         * @return      The bits as an unsigned long
         * @throws IOException  Thrown if error reading input stream
         */

        public long ReadRawULong(int bits)
        {
            long val = 0;
            for (int i = 0; i < bits; i++)
            {
                val = ReadBitToLong(val);
            }
            return val;
        }

        /**
         * read bits into an unsigned little endian integer.
         * @return      The bits as an unsigned integer
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadRawIntLittleEndian()
        {
            int x32 = ReadRawUInt(8);
            int x8 = ReadRawUInt(8);
            x32 |= (x8 << 8);
            x8 = ReadRawUInt(8);
            x32 |= (x8 << 16);
            x8 = ReadRawUInt(8);
            x32 |= (x8 << 24);
            return x32;
        }

        /**
         * Read a block of bytes (aligned) without updating the CRC value.
         * @param val   The array to receive the bytes. If null, no bytes are returned
         * @param nvals The number of bytes to read
         * @throws IOException  Thrown if error reading input stream
         */

        public void ReadByteBlockAlignedNoCRC(byte[] val, int nvals)
        {
            int destlength = nvals;
            while (nvals > 0)
            {
                int chunk = Math.Min(nvals, putByte - getByte);
                if (chunk == 0)
                {
                    ReadFromStream();
                }
                else
                {
                    if (val != null) Array.Copy(buffer, getByte, val, destlength - nvals, chunk);
                    nvals -= chunk;
                    getByte += chunk;
                    //totalConsumedBits = (getByte << BITS_PER_BLURB_LOG2);
                    availBits -= (chunk << BITS_PER_BLURB_LOG2);
                    totalBitsRead += (chunk << BITS_PER_BLURB_LOG2);
                }
            }
        }

        /**
         * Read and count the number of zero bits.
         * @return  The number of zero bits read
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadUnaryUnsigned()
        {
            int val = 0;
            while (true)
            {
                int bit = ReadBit();
                if (bit != 0) break;
                val++;
            }
            return val;
        }

        /**
         * Read a Rice Signal Block.
         * @param vals  The values to be returned
         * @param pos   The starting position in the vals array
         * @param nvals The number of values to return
         * @param parameter The Rice parameter
         * @throws IOException  On read error
         */

        public void ReadRiceSignedBlock(ref int[] vals, int pos, int nvals, int parameter)
        {
            int j, valI = 0;
            int cbits = 0, uval = 0, msbs = 0, lsbsLeft = 0;
            byte blurb, saveBlurb;
            int state = 0; // 0 = getting unary MSBs, 1 = getting binary LSBs
            if (nvals == 0) return;
            int i = getByte;

            long startBits = getByte*8 + getBit;

            // We unroll the main loop to take care of partially consumed blurbs here.
            if (getBit > 0)
            {
                saveBlurb = blurb = buffer[i];
                cbits = getBit;
                blurb <<= cbits;
                while (true)
                {
                    if (state == 0)
                    {
                        if (blurb != 0)
                        {
                            for (j = 0; (blurb & BLURB_TOP_BIT_ONE) == 0; j++)
                                blurb <<= 1;
                            msbs += j;

                            // dispose of the unary end bit
                            blurb <<= 1;
                            j++;
                            cbits += j;
                            uval = 0;
                            lsbsLeft = parameter;
                            state++;
                            //totalBitsRead += msbs;
                            if (cbits == BITS_PER_BLURB)
                            {
                                cbits = 0;
                                readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                                break;
                            }
                        }
                        else
                        {
                            msbs += BITS_PER_BLURB - cbits;
                            cbits = 0;
                            readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                            //totalBitsRead += msbs;
                            break;
                        }
                    }
                    else
                    {
                        int availableBits = BITS_PER_BLURB - cbits;
                        if (lsbsLeft >= availableBits)
                        {
                            uval <<= availableBits;
                            uval |= ((blurb & 0xff) >> cbits);
                            cbits = 0;
                            readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                            //totalBitsRead += availableBits;
                            if (lsbsLeft == availableBits)
                            {
                                // compose the value
                                uval |= (msbs << parameter);
                                if ((uval & 1) != 0)
                                    vals[pos + valI++] = -((uval >> 1)) - 1;
                                else
                                    vals[pos + valI++] = (uval >> 1);
                                if (valI == nvals)
                                    break;
                                msbs = 0;
                                state = 0;
                            }
                            lsbsLeft -= availableBits;
                            break;
                        }
                        else
                        {
                            uval <<= lsbsLeft;
                            uval |= ((blurb & 0xff) >> (BITS_PER_BLURB - lsbsLeft));
                            blurb <<= lsbsLeft;
                            cbits += lsbsLeft;
                            //totalBitsRead += lsbsLeft;
                            // compose the value
                            uval |= (msbs << parameter);
                            if ((uval & 1) != 0)
                                vals[pos + valI++] = -((uval >> 1)) - 1;
                            else
                                vals[pos + valI++] = (uval >> 1);
                            if (valI == nvals)
                            {
                                // back up one if we exited the for loop because we
                                // read all nvals but the end came in the middle of
                                // a blurb
                                i--;
                                break;
                            }
                            msbs = 0;
                            state = 0;
                        }
                    }
                }
                i++;
                getByte = i;
                getBit = cbits;
                //totalConsumedBits = (i << BITS_PER_BLURB_LOG2) | cbits;
                //totalBitsRead += (BITS_PER_BLURB) | cbits;
            }

            // Now that we are blurb-aligned the logic is slightly simpler
            while (valI < nvals)
            {
                for (; i < putByte && valI < nvals; i++)
                {
                    saveBlurb = blurb = buffer[i];
                    cbits = 0;
                    while (true)
                    {
                        if (state == 0)
                        {
                            if (blurb != 0)
                            {
                                for (j = 0; (blurb & BLURB_TOP_BIT_ONE) == 0; j++) blurb <<= 1;
                                msbs += j;
                                // dispose of the unary end bit
                                blurb <<= 1;
                                j++;
                                cbits += j;
                                uval = 0;
                                lsbsLeft = parameter;
                                state++;
                                //totalBitsRead += msbs;
                                if (cbits == BITS_PER_BLURB)
                                {
                                    cbits = 0;
                                    readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                                    break;
                                }
                            }
                            else
                            {
                                msbs += BITS_PER_BLURB - cbits;
                                cbits = 0;
                                readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                                //totalBitsRead += msbs;
                                break;
                            }
                        }
                        else
                        {
                            int availableBits = BITS_PER_BLURB - cbits;
                            if (lsbsLeft >= availableBits)
                            {
                                uval <<= availableBits;
                                uval |= ((blurb & 0xff) >> cbits);
                                cbits = 0;
                                readCRC16 = Crc16.Update(saveBlurb, readCRC16);
                                //totalBitsRead += availableBits;
                                if (lsbsLeft == availableBits)
                                {
                                    // compose the value
                                    uval |= (msbs << parameter);
                                    if ((uval & 1) != 0)
                                        vals[pos + valI++] = -((uval >> 1)) - 1;
                                    else
                                        vals[pos + valI++] = (uval >> 1);
                                    if (valI == nvals)
                                        break;
                                    msbs = 0;
                                    state = 0;
                                }
                                lsbsLeft -= availableBits;
                                break;
                            }
                            else
                            {
                                uval <<= lsbsLeft;
                                uval |= ((blurb & 0xff) >> (BITS_PER_BLURB - lsbsLeft));
                                blurb <<= lsbsLeft;
                                cbits += lsbsLeft;
                                //totalBitsRead += lsbsLeft;
                                // compose the value
                                uval |= (msbs << parameter);
                                if ((uval & 1) != 0)
                                    vals[pos + valI++] = -((uval >> 1)) - 1;
                                else
                                    vals[pos + valI++] = (uval >> 1);
                                if (valI == nvals)
                                {
                                    // back up one if we exited the for loop because
                                    // we read all nvals but the end came in the
                                    // middle of a blurb
                                    i--;
                                    break;
                                }
                                msbs = 0;
                                state = 0;
                            }
                        }
                    }
                }
                getByte = i;
                getBit = cbits;
                //totalConsumedBits = (i << BITS_PER_BLURB_LOG2) | cbits;
                //totalBitsRead += (BITS_PER_BLURB) | cbits;
                if (valI < nvals)
                {
                    int endBits = getByte*8 + getBit;
                    //System.out.println("SE0 "+startBits+" "+endBits);
                    totalBitsRead += endBits - startBits;
                    availBits -= endBits - startBits;
                    ReadFromStream();
                    // these must be zero because we can only get here if we got to
                    // the end of the buffer
                    i = 0;
                    startBits = getByte*8 + getBit;
                }
            }

            int endBit = getByte*8 + getBit;
            //System.out.println("SE1 "+startBits+" "+endBits);
            totalBitsRead += endBit - startBits;
            availBits -= endBit - startBits;
        }

        /**
         * read UTF8 integer.
         * on return, if *val == 0xffffffff then the utf-8 sequence was invalid, but
         * the return value will be true
         * @param raw   The raw bytes read (output). If null, no bytes are returned
         * @return      The integer read
         * @throws IOException  Thrown if error reading input stream
         */

        public int ReadUTF8Int(ByteData raw)
        {
            int val;
            int v = 0;
            int x;
            int i;
            x = ReadRawUInt(8);
            if (raw != null) raw.Append((byte) x);
            if ((x & 0x80) == 0)
            {
                // 0xxxxxxx
                v = x;
                i = 0;
            }
            else if (((x & 0xC0) != 0) && ((x & 0x20) == 0))
            {
                // 110xxxxx
                v = x & 0x1F;
                i = 1;
            }
            else if (((x & 0xE0) != 0) && ((x & 0x10) == 0))
            {
                // 1110xxxx
                v = x & 0x0F;
                i = 2;
            }
            else if (((x & 0xF0) != 0) && ((x & 0x08) == 0))
            {
                // 11110xxx
                v = x & 0x07;
                i = 3;
            }
            else if (((x & 0xF8) != 0) && ((x & 0x04) == 0))
            {
                // 111110xx
                v = x & 0x03;
                i = 4;
            }
            else if (((x & 0xFC) != 0) && ((x & 0x02) == 0))
            {
                // 1111110x
                v = x & 0x01;
                i = 5;
            }
            else
            {
                val = 0xfffffff;
                return val;
            }
            for (; i > 0; i--)
            {
                x = PeekRawUInt(8);
                if (((x & 0x80) == 0) || ((x & 0x40) != 0))
                {
                    // 10xxxxxx
                    val = 0xfffffff;
                    return val;
                }
                x = ReadRawUInt(8);
                if (raw != null)
                    raw.Append((byte) x);
                v <<= 6;
                v |= (x & 0x3F);
            }
            val = v;
            return val;
        }

        /**
         * read UTF long.
         * on return, if *val == 0xffffffffffffffff then the utf-8 sequence was
         * invalid, but the return value will be true
         * @param raw   The raw bytes read (output). If null, no bytes are returned
         * @return      The long read
         * @throws IOException  Thrown if error reading input stream
         */

        public long ReadUTF8Long(ByteData raw)
        {
            long v = 0;
            int x;
            int i;
            long val;
            x = ReadRawUInt(8);
            if (raw != null)
                raw.Append((byte) x);
            if (((x & 0x80) == 0))
            {
                // 0xxxxxxx
                v = x;
                i = 0;
            }
            else if (((x & 0xC0) != 0) && ((x & 0x20) == 0))
            {
                // 110xxxxx
                v = x & 0x1F;
                i = 1;
            }
            else if (((x & 0xE0) != 0) && ((x & 0x10) == 0))
            {
                // 1110xxxx
                v = x & 0x0F;
                i = 2;
            }
            else if (((x & 0xF0) != 0) && ((x & 0x08) == 0))
            {
                // 11110xxx
                v = x & 0x07;
                i = 3;
            }
            else if (((x & 0xF8) != 0) && ((x & 0x04) == 0))
            {
                // 111110xx
                v = x & 0x03;
                i = 4;
            }
            else if (((x & 0xFC) != 0) && ((x & 0x02) == 0))
            {
                // 1111110x
                v = x & 0x01;
                i = 5;
            }
            else if (((x & 0xFE) != 0) && ((x & 0x01) == 0))
            {
                // 11111110
                v = 0;
                i = 6;
            }
            else
            {
                val = 0xfffffffffffffffL;
                return val;
            }
            for (; i > 0; i--)
            {
                x = PeekRawUInt(8);
                if (((x & 0x80) == 0) || ((x & 0x40) != 0))
                {
                    // 10xxxxxx
                    val = 0xfffffffffffffffL;
                    return val;
                }
                x = ReadRawUInt(8);
                if (raw != null)
                    raw.Append((byte) x);
                v <<= 6;
                v |= (x & 0x3F);
            }
            val = v;
            return val;
        }

        /**
         * Total Blurbs read.
         * @return Returns the total blurbs read.
         */

        public long GetTotalBytesRead()
        {
            return ((totalBitsRead + 7)/8);
        }
    }
}