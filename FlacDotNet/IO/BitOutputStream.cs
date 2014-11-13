using System;
using System.IO;
using FlacDotNet.Util;

namespace FlacDotNet.IO
{
    internal class BitOutputStream
    {
        private const int BITS_PER_BLURB = 8;
        //private static final int BITS_PER_BLURB_LOG2 = 3;
        //private static final int BYTES_PER_BLURB = 1;
        //private static final byte BLURB_TOP_BIT_ONE = ((byte) 0x80);
        private static readonly long[] MASK32 =
        {
            0, 0x0000000000000001, 0x0000000000000003, 0x0000000000000007, 0x000000000000000F,
            0x000000000000001F, 0x000000000000003F, 0x000000000000007F, 0x00000000000000FF, 0x00000000000001FF,
            0x00000000000003FF,
            0x00000000000007FF, 0x0000000000000FFF, 0x0000000000001FFF, 0x0000000000003FFF, 0x0000000000007FFF,
            0x000000000000FFFF,
            0x000000000001FFFF, 0x000000000003FFFF, 0x000000000007FFFF, 0x00000000000FFFFF, 0x00000000001FFFFF,
            0x00000000003FFFFF,
            0x00000000007FFFFF, 0x0000000000FFFFFF, 0x0000000001FFFFFF, 0x0000000003FFFFFF, 0x0000000007FFFFFF,
            0x000000000FFFFFFF,
            0x000000001FFFFFFF, 0x000000003FFFFFFF, 0x000000007FFFFFFF, 0x00000000FFFFFFFF, 0x00000001FFFFFFFFL,
            0x00000003FFFFFFFFL, 0x00000007FFFFFFFFL, 0x0000000FFFFFFFFFL, 0x0000001FFFFFFFFFL, 0x0000003FFFFFFFFFL,
            0x0000007FFFFFFFFFL, 0x000000FFFFFFFFFFL, 0x000001FFFFFFFFFFL, 0x000003FFFFFFFFFFL, 0x000007FFFFFFFFFFL,
            0x00000FFFFFFFFFFFL, 0x00001FFFFFFFFFFFL, 0x00003FFFFFFFFFFFL, 0x00007FFFFFFFFFFFL, 0x0000FFFFFFFFFFFFL,
            0x0001FFFFFFFFFFFFL, 0x0003FFFFFFFFFFFFL, 0x0007FFFFFFFFFFFFL, 0x000FFFFFFFFFFFFFL, 0x001FFFFFFFFFFFFFL,
            0x003FFFFFFFFFFFFFL, 0x007FFFFFFFFFFFFFL, 0x00FFFFFFFFFFFFFFL, 0x01FFFFFFFFFFFFFFL, 0x03FFFFFFFFFFFFFFL,
            0x07FFFFFFFFFFFFFFL, 0x0FFFFFFFFFFFFFFFL, 0x1FFFFFFFFFFFFFFFL, 0x3FFFFFFFFFFFFFFFL, 0x7FFFFFFFFFFFFFFFL,
            0xFFFFFFFFFFFFFFFL
        };

        private readonly Stream os;

        private byte[] buffer = new byte[0];
        private int consumedBits;
        private int consumedBlurbs;
        private int outBits;
        private int outBlurbs;
        private int outCapacity; // in blurbs
        private short readCRC16;
        private int totalBits; // must always == BITS_PER_BLURB*blurbs+bits
        private int totalConsumedBits;

        public BitOutputStream(Stream os)
        {
            this.os = os;
        }

        /**
     * The constructor.
     */

        public BitOutputStream()
        {
        }

        /*
     * WATCHOUT: The current implentation is not friendly to shrinking, i.e. it
     * does not shift left what is consumed, it just chops off the end, whether
     * there is unconsumed data there or not. This is OK because currently we
     * never shrink the buffer, but if this ever changes, we'll have to do some
     * fixups here.
     */

        private bool Resize(int newCapacity)
        {
            if (outCapacity >= newCapacity) return true;
            var newBuffer = new byte[newCapacity];
            Array.Copy(buffer, 0, newBuffer, 0, Math.Min(outBlurbs + ((outBits != 0) ? 1 : 0), newCapacity));
            if (newCapacity < outBlurbs + ((outBits != 0) ? 1 : 0))
            {
                outBlurbs = newCapacity;
                outBits = 0;
                totalBits = newCapacity << 3;
            }
            if (newCapacity < consumedBlurbs + ((consumedBits != 0) ? 1 : 0))
            {
                consumedBlurbs = newCapacity;
                consumedBits = 0;
                totalConsumedBits = newCapacity << 3;
            }
            buffer = newBuffer;
            outCapacity = newCapacity;
            return true;
        }

        private bool Grow(int minBlurbsToAdd)
        {
            int newCapacity = Math.Max(outCapacity*2, outCapacity + minBlurbsToAdd);
            return Resize(newCapacity);
        }

        private bool EnsureSize(int bitsToAdd)
        {
            if ((outCapacity << 3) < totalBits + bitsToAdd)
            {
                return Grow((bitsToAdd >> 3) + 2);
            }
            return true;
        }


        public bool ConcatenateAligned(BitOutputStream src)
        {
            int bitsToAdd = src.totalBits - src.totalConsumedBits;
            if (bitsToAdd == 0) return true;
            if (outBits != src.consumedBits) return false;
            if (!EnsureSize(bitsToAdd)) return false;
            if (outBits == 0)
            {
                Array.Copy(src.buffer, src.consumedBlurbs, buffer, outBlurbs,
                           (src.outBlurbs - src.consumedBlurbs + ((src.outBits != 0) ? 1 : 0)));
            }
            else if (outBits + bitsToAdd > BITS_PER_BLURB)
            {
                buffer[outBlurbs] <<= (BITS_PER_BLURB - outBits);
                buffer[outBlurbs] |= (byte) (src.buffer[src.consumedBlurbs] & ((1 << (BITS_PER_BLURB - outBits)) - 1));
                Array.Copy(src.buffer, src.consumedBlurbs + 1, buffer, outBlurbs + 11,
                           (src.outBlurbs - src.consumedBlurbs - 1 + ((src.outBits != 0) ? 1 : 0)));
            }
            else
            {
                buffer[outBlurbs] <<= bitsToAdd;
                buffer[outBlurbs] |= (byte) (src.buffer[src.consumedBlurbs] & ((1 << bitsToAdd) - 1));
            }
            outBits = src.outBits;
            totalBits += bitsToAdd;
            outBlurbs = totalBits/BITS_PER_BLURB;
            return true;
        }

        /**
     * Reset the read CRC-16 value.
     * @param seed  The initial CRC-16 value
     */

        public void ResetReadCRC16(short seed)
        {
            readCRC16 = seed;
        }

        /**
     * return the read CRC-16 value.
     * @return  The read CRC-16 value
     */

        public short GetReadCRC16()
        {
            return readCRC16;
        }

        /**
     * return the write CRC-16 value.
     * @return The write CRC-16 value
     */

        public int GetWriteCRC16()
        {
            return Crc16.Calc(buffer, outBlurbs);
        }

        /**
     * return the write CRC-8 value.
     * @return  The write CRC-8 value
     */

        public byte GetWriteCRC8()
        {
            return Crc8.Calc(buffer, outBlurbs);
        }

        /**
     * Test if the Bit Stream is byte aligned.
     * @return  True of bit stream is byte aligned
     */

        public bool IsByteAligned()
        {
            return ((outBits & 7) == 0);
        }

        /**
     * Test if the Bit Stream consumed bits is byte aligned.
     * @return  True of bit stream consumed bits is byte aligned
     */

        public bool IsConsumedByteAligned()
        {
            return ((consumedBits & 7) == 0);
        }

        /**
     * return the number of bits to read to align the byte.
     * @return  The number of bits to align the byte
     */

        public int BitsLeftForByteAlignment()
        {
            return 8 - (consumedBits & 7);
        }

        /**
     * return the number of bytes left to read.
     * @return  The number of bytes left to read
     */

        public int GetInputBytesUnconsumed()
        {
            return (totalBits - totalConsumedBits) >> 3;
        }

        /**
     * Write zero bits.
     * @param bits  The number of zero bits to write
     * @throws IOException  On write error
     */

        public void WriteZeroes(int bits)
        {
            if (bits == 0) return;
            if (!EnsureSize(bits)) throw new IOException("Memory Allocation Error");
            totalBits += bits;
            while (bits > 0)
            {
                int n = Math.Min(BITS_PER_BLURB - outBits, bits);
                buffer[outBlurbs] <<= n;
                bits -= n;
                outBits += n;
                if (outBits == BITS_PER_BLURB)
                {
                    outBlurbs++;
                    outBits = 0;
                }
            }
        }

        /**
     * Write a true/false integer.
     * @param val   The true/false value
     * @param bits  The bit size to write
     * @throws IOException  On write error
     */

        public void WriteRawUInt(bool val, int bits)
        {
            WriteRawUInt((val) ? 1 : 0, bits);
        }

        public void WriteRawUInt(int val, int bits)
        {
            if (bits == 0) return;

            // inline the size check so we don't incure a function call unnecessarily
            if ((outCapacity << 3) < totalBits + bits)
            {
                if (!EnsureSize(bits)) throw new IOException("Memory allocation error");
            }

            // zero-out unused bits; WATCHOUT: other code relies on this, so this needs to stay
            if (bits < 32) val &= (byte) (~(0xffffffff << bits)); // zero-out unused bits
            totalBits += bits;
            while (bits > 0)
            {
                int n = BITS_PER_BLURB - outBits;
                if (n == BITS_PER_BLURB)
                {
                    // i.e. outBits == 0
                    if (bits < BITS_PER_BLURB)
                    {
                        buffer[outBlurbs] = (byte) val;
                        outBits = bits;
                        break;
                    }
                    else if (bits == BITS_PER_BLURB)
                    {
                        buffer[outBlurbs++] = (byte) val;
                        break;
                    }
                    else
                    {
                        int k = bits - BITS_PER_BLURB;
                        buffer[outBlurbs++] = (byte) (val >> k);

                        // we know k < 32 so no need to protect against the gcc bug mentioned above
                        val &= (byte) (~(0xffffffff << k));
                        bits -= BITS_PER_BLURB;
                    }
                }
                else if (bits <= n)
                {
                    buffer[outBlurbs] <<= bits;
                    buffer[outBlurbs] |= (byte) val;
                    if (bits == n)
                    {
                        outBlurbs++;
                        outBits = 0;
                    }
                    else
                        outBits += bits;
                    break;
                }
                else
                {
                    int k = bits - n;
                    buffer[outBlurbs] <<= n;
                    buffer[outBlurbs] |= (byte) (val >> k);

                    // we know n > 0 so k < 32 so no need to protect against the gcc bug mentioned above
                    val &= (byte) (~(0xffffffff << k));
                    bits -= n;
                    outBlurbs++;
                    outBits = 0;
                }
            }
        }

        public void WriteRawInt(int val, int bits)
        {
            WriteRawUInt(val, bits);
        }

        public void WriteRawULong(long val, int bits)
        {
            if (bits == 0) return;
            if (!EnsureSize(bits)) throw new IOException("Memory Allocate Error");
            val &= MASK32[bits];
            totalBits += bits;
            while (bits > 0)
            {
                if (outBits == 0)
                {
                    if (bits < BITS_PER_BLURB)
                    {
                        buffer[outBlurbs] = (byte) val;
                        outBits = bits;
                        break;
                    }
                    else if (bits == BITS_PER_BLURB)
                    {
                        buffer[outBlurbs++] = (byte) val;
                        break;
                    }
                    else
                    {
                        int k = bits - BITS_PER_BLURB;
                        buffer[outBlurbs++] = (byte) (val >> k);

                        // we know k < 64 so no need to protect against the gcc bug mentioned above
                        val &= (byte) (~(0xffffffffffffffffL << k));
                        bits -= BITS_PER_BLURB;
                    }
                }
                else
                {
                    int n = Math.Min(BITS_PER_BLURB - outBits, bits);
                    int k = bits - n;
                    buffer[outBlurbs] <<= n;
                    buffer[outBlurbs] |= (byte) (val >> k);

                    // we know n > 0 so k < 64 so no need to protect against the gcc bug mentioned above
                    val &= (byte) (~(0xffffffffffffffffL << k));
                    bits -= n;
                    outBits += n;
                    if (outBits == BITS_PER_BLURB)
                    {
                        outBlurbs++;
                        outBits = 0;
                    }
                }
            }
        }

        public void WriteRawUIntLittleEndian(int val)
        {
            // NOTE: we rely on the fact that write_raw_uint32() masks out the unused bits
            WriteRawUInt(val, 8);
            WriteRawUInt(val >> 8, 8);
            WriteRawUInt(val >> 16, 8);
            WriteRawUInt(val >> 24, 8);
        }

        public void WriteByteBlock(byte[] vals, int nvals)
        {
            // this could be faster but currently we don't need it to be
            for (int i = 0; i < nvals; i++)
            {
                WriteRawUInt((vals[i]), 8);
            }
        }

        public void WriteUnaryUnsigned(int val)
        {
            if (val < 32)
                WriteRawUInt(1, ++val);
            else if (val < 64)
                WriteRawULong(1, ++val);
            else
            {
                WriteZeroes(val);
                WriteRawUInt(1, 1);
            }
        }

        public int RiceBits(int val, int parameter)
        {
            int msbs, uval;
            // fold signed to unsigned
            if (val < 0)
            {
                // equivalent to (unsigned)(((--val) < < 1) - 1); but without the overflow problem at MININT
                uval = (((-(++val)) << 1) + 1);
            }
            else
            {
                uval = (val << 1);
            }
            msbs = uval >> parameter;
            return 1 + parameter + msbs;
        }

        public void WriteRiceSigned(int val, int parameter)
        {
            int totalBits;
            int interestingBits;
            int msbs;
            int uval;
            int pattern;

            // fold signed to unsigned
            if (val < 0)
            {
                // equivalent to (unsigned)(((--val) < < 1) - 1); but without the overflow problem at MININT
                uval = (((-(++val)) << 1) + 1);
            }
            else
            {
                uval = (val << 1);
            }
            msbs = uval >> parameter;
            interestingBits = 1 + parameter;
            totalBits = interestingBits + msbs;
            pattern = 1 << parameter; /* the unary end bit */
            pattern |= (uval & ((1 << parameter) - 1)); /* the binary LSBs */
            if (totalBits <= 32)
            {
                WriteRawUInt(pattern, totalBits);
            }
            else
            {
                /* write the unary MSBs */
                WriteZeroes(msbs);
                /* write the unary end bit and binary LSBs */
                WriteRawUInt(pattern, interestingBits);
            }
        }

        public void WriteUTF8UInt(int val)
        {
            if (val < 0x80)
            {
                WriteRawUInt(val, 8);
            }
            else if (val < 0x800)
            {
                WriteRawUInt(0xC0 | (val >> 6), 8);
                WriteRawUInt(0x80 | (val & 0x3F), 8);
            }
            else if (val < 0x10000)
            {
                WriteRawUInt(0xE0 | (val >> 12), 8);
                WriteRawUInt(0x80 | ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (val & 0x3F), 8);
            }
            else if (val < 0x200000)
            {
                WriteRawUInt(0xF0 | (val >> 18), 8);
                WriteRawUInt(0x80 | ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (val & 0x3F), 8);
            }
            else if (val < 0x4000000)
            {
                WriteRawUInt(0xF8 | (val >> 24), 8);
                WriteRawUInt(0x80 | ((val >> 18) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (val & 0x3F), 8);
            }
            else
            {
                WriteRawUInt(0xFC | (val >> 30), 8);
                WriteRawUInt(0x80 | ((val >> 24) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 18) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (val & 0x3F), 8);
            }
        }

        public void WriteUTF8ULong(long val)
        {
            if (val < 0x80)
            {
                WriteRawUInt((int) val, 8);
            }
            else if (val < 0x800)
            {
                WriteRawUInt(0xC0 | (int) (val >> 6), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
            else if (val < 0x10000)
            {
                WriteRawUInt(0xE0 | (int) (val >> 12), 8);
                WriteRawUInt(0x80 | (int) ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
            else if (val < 0x200000)
            {
                WriteRawUInt(0xF0 | (int) (val >> 18), 8);
                WriteRawUInt(0x80 | (int) ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
            else if (val < 0x4000000)
            {
                WriteRawUInt(0xF8 | (int) (val >> 24), 8);
                WriteRawUInt(0x80 | (int) ((val >> 18) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
            else if (val < 0x80000000)
            {
                WriteRawUInt(0xFC | (int) (val >> 30), 8);
                WriteRawUInt(0x80 | (int) ((val >> 24) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 18) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
            else
            {
                WriteRawUInt(0xFE, 8);
                WriteRawUInt(0x80 | (int) ((val >> 30) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 24) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 18) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 12) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) ((val >> 6) & 0x3F), 8);
                WriteRawUInt(0x80 | (int) (val & 0x3F), 8);
            }
        }

        /**
     * Write zero bits to byte boundry.
     * @throws IOException  On error writing to bit stream
     */

        public void ZeroPadToByteBoundary()
        {
            // 0-pad to byte boundary
            if ((outBits & 7) != 0) WriteZeroes(8 - (outBits & 7));
        }

        /**
     * Flush bit stream after aligning byte boundry.
     * @throws IOException  On error writing.
     */

        public void FlushByteAligned()
        {
            ZeroPadToByteBoundary();
            if (outBlurbs == 0) return;
            os.Write(buffer, 0, outBlurbs);
            outBlurbs = 0;
        }


        public int GetTotalBits()
        {
            return totalBits;
        }

        /**
     * Returns the totalBlurbs.
     * @return Returns the totalBlurbs.
     */

        public int GetTotalBlurbs()
        {
            return (totalBits + 7)/8;
        }
    }
}