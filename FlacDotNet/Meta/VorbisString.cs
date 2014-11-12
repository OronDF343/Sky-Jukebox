using System;
using System.Text;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    public class VorbisString
    {
        protected byte[] Entry;

        public VorbisString(BitInputStream inputStream)
        {
            int elen = inputStream.ReadRawIntLittleEndian();
            if (elen == 0) return;
            Entry = new byte[elen];
            inputStream.ReadByteBlockAlignedNoCRC(Entry, Entry.Length);
        }

        public override String ToString()
        {
            String s;
            try
            {
                s = Encoding.UTF8.GetString(Entry, 0, Entry.Length);
            }
            catch (Exception)
            {
                s = "";
            }
            return s;
        }
    }
}