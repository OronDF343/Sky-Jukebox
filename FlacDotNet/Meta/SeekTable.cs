using System;
using System.Text;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class SeekTable : Metadata
    {
        private const int SEEKPOINT_LENGTH_BYTES = 18;

        public SeekPoint[] Points { get; protected set; }

        public SeekTable(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            int numPoints = length/SEEKPOINT_LENGTH_BYTES;

            Points = new SeekPoint[numPoints];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new SeekPoint(inputStream);
            }
            length -= (length*SEEKPOINT_LENGTH_BYTES);

            // if there is a partial point left, skip over it
            if (length > 0) inputStream.ReadByteBlockAlignedNoCRC(null, length);
        }


        public SeekTable(SeekPoint[] points, bool isLast)
            : base(isLast)
        {
            Points = points;
        }

        public void Write(BitOutputStream os, bool isLast)
        {
            os.WriteRawUInt(isLast, STREAM_METADATA_IS_LAST_LEN);
            os.WriteRawUInt(METADATA_TYPE_SEEKTABLE, STREAM_METADATA_TYPE_LEN);
            os.WriteRawUInt(CalcLength(), STREAM_METADATA_LENGTH_LEN);

            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].Write(os);
            }

            os.FlushByteAligned();
        }

        public int CalcLength()
        {
            return Points.Length*SEEKPOINT_LENGTH_BYTES;
        }

        public SeekPoint GETSeekPoint(int idx)
        {
            if (idx < 0 || idx >= Points.Length) return null;
            return Points[idx];
        }

        public int NumberOfPoints()
        {
            return Points.Length;
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SeekTable: points=" + Points.Length + "\n");
            for (int i = 0; i < Points.Length; i++)
            {
                sb.Append("\tPoint " + Points[i] + "\n");
            }

            return sb.ToString();
        }
    }
}