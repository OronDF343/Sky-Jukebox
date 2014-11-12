using System;
using System.Text;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    public class Picture : Metadata
    {
        private readonly String _descString; // UTF-8
        private readonly int _descStringByteCount;
        private readonly String _mimeString; //ASCII 0x20 to 0x7e or --> (data is URL)
        private readonly int _mimeTypeByteCount;
        private readonly int _picBitsPerPixel;
        private readonly int _picByteCount;
        private readonly int _picColorCount; // for GIF, else 0
        private readonly int _picPixelHeight;
        private readonly int _picPixelWidth;
        private readonly int _pictureType;

        public byte[] Image { get; private set; }


        /**
         * The constructor.
         * @param is                The InputBitStream
         * @param length            Length of the record
         * @param isLast            True if this is the last Metadata block in the chain
         * @throws IOException      Thrown if error reading from InputBitStream
         */

        public Picture(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            int usedBits = 0;

            _pictureType = inputStream.ReadRawUInt(32);
            usedBits += 32;

            _mimeTypeByteCount = inputStream.ReadRawUInt(32);
            usedBits += 32;

            var data = new byte[_mimeTypeByteCount];
            inputStream.ReadByteBlockAlignedNoCRC(data, _mimeTypeByteCount);
            usedBits += _mimeTypeByteCount*8;

            _mimeString = Encoding.UTF8.GetString(data, 0, data.Length); // convert to a string

            _descStringByteCount = inputStream.ReadRawUInt(32);
            usedBits += 32;

            if (_descStringByteCount != 0)
            {
                data = new byte[_descStringByteCount];
                inputStream.ReadByteBlockAlignedNoCRC(data, _descStringByteCount);
                try
                {
                    _descString = Encoding.UTF8.GetString(data, 0, data.Length);
                }
                catch (DecoderFallbackException)
                {
                }
                usedBits += 32;
            }
            else
            {
                _descString = "";
            }

            _picPixelWidth = inputStream.ReadRawUInt(32);
            usedBits += 32;

            _picPixelHeight = inputStream.ReadRawUInt(32);
            usedBits += 32;

            _picBitsPerPixel = inputStream.ReadRawUInt(32);
            usedBits += 32;

            _picColorCount = inputStream.ReadRawUInt(32);
            usedBits += 32;

            _picByteCount = inputStream.ReadRawUInt(32);
            usedBits += 32;

            //get the image now
            Image = new byte[_picByteCount];
            inputStream.ReadByteBlockAlignedNoCRC(Image, _picByteCount);
            usedBits += _picByteCount*8;

            // skip the rest of the block if any
            length -= (usedBits/8);
            inputStream.ReadByteBlockAlignedNoCRC(null, length);
        }

        public override String ToString()
        {
            return "Picture: "
                   + " Type=" + _pictureType
                   + " MIME type=" + _mimeString
                   + " Description=\"" + _descString + "\""
                   + " Pixels (WxH)=" + _picPixelWidth + "x" + _picPixelHeight
                   + " Color Depth=" + _picBitsPerPixel
                   + " Color Count=" + _picColorCount
                   + " Picture Size (bytes)=" + _picByteCount;
        }
    }
}