using System;
using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class SeekPoint
    {
        private const int SEEKPOINT_SAMPLE_NUMBER_LEN = 64; // bits
        private const int SEEKPOINT_STREAM_OFFSET_LEN = 64; // bits
        private const int SEEKPOINT_FRAME_SAMPLES_LEN = 16; // bits

        public SeekPoint(BitInputStream inputStream)
        {
            SampleNumber = inputStream.ReadRawULong(SEEKPOINT_SAMPLE_NUMBER_LEN);
            StreamOffset = inputStream.ReadRawULong(SEEKPOINT_STREAM_OFFSET_LEN);
            FrameSamples = inputStream.ReadRawUInt(SEEKPOINT_FRAME_SAMPLES_LEN);
        }

        /**
     * The constructor.
     * @param sampleNumber  The sample number of the target frame
     * @param streamOffset  The offset, in bytes, of the target frame with respect to beginning of the first frame
     * @param frameSamples  The number of samples in the target frame
     */

        public SeekPoint(long sampleNumber, long streamOffset, int frameSamples)
        {
            SampleNumber = sampleNumber;
            StreamOffset = streamOffset;
            FrameSamples = frameSamples;
        }

        internal long SampleNumber { get; private set; } // The sample number of the target frame.
        internal long StreamOffset { get; set; }
        // The offset, in bytes, of the target frame with respect to beginning of the first frame.
        internal int FrameSamples { get; private set; } // The number of samples in the target frame.

        /**
     * The constructor.
     * @param is                The InputBitStream
     * @throws IOException      Thrown if error reading from InputBitStream
     */

        /**
     * Write out an individual seek point.
     * @param os    The output stream
     * @throws IOException  Thrown if error writing data
     */

        public void Write(BitOutputStream os)
        {
            os.WriteRawULong(SampleNumber, SEEKPOINT_SAMPLE_NUMBER_LEN);
            os.WriteRawULong(StreamOffset, SEEKPOINT_STREAM_OFFSET_LEN);
            os.WriteRawUInt(FrameSamples, SEEKPOINT_FRAME_SAMPLES_LEN);
        }

        /**
     * @see java.lang.Object#toString()
     */

        public override String ToString()
        {
            return "sampleNumber=" + SampleNumber + " streamOffset=" + StreamOffset + " frameSamples=" + FrameSamples;
        }
    }
}