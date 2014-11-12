using System;
using System.Linq;
using System.Text;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    public class VorbisComment : Metadata
    {
        public VorbisString[] Comments { get; protected set; }
        protected int NumComments = 0;
        protected byte[] VendorString = new byte[0];

        /**
         * The constructor.
         * @param is                The InputBitStream
         * @param length            Length of the record
         * @param isLast            True if this is the last Metadata block in the chain
         * @throws IOException      Thrown if error reading from InputBitStream
         */

        public VorbisComment(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            // read vendor string
            int len = inputStream.ReadRawIntLittleEndian();
            VendorString = new byte[len];
            inputStream.ReadByteBlockAlignedNoCRC(VendorString, VendorString.Length);

            // read comments
            NumComments = inputStream.ReadRawIntLittleEndian();
            if (NumComments > 0) Comments = new VorbisString[NumComments];
            for (int i = 0; i < NumComments; i++)
            {
                Comments[i] = new VorbisString(inputStream);
            }
        }

        public override String ToString()
        {
            var sb = new StringBuilder("VendorString '" + VendorString + "'\n");
            sb.Append("VorbisComment (count=" + NumComments + ")");

            for (int i = 0; i < NumComments; i++)
            {
                sb.Append("\n\t" + Comments[i]);
            }

            return sb.ToString();
        }

        public String[] GetCommentByName(String key)
        {
            if (key == null) return null;
            return (from t in Comments
                    select t.ToString()
                    into comment let eqpos = comment.IndexOf((char) 0x3D) where eqpos != -1
                    where comment.Substring(0, eqpos).Equals(key, StringComparison.OrdinalIgnoreCase)
                    select comment.Substring(eqpos + 1, comment.Length-eqpos-1)).ToArray();
            //return null;
        }
    }
}