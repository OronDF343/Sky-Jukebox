namespace FlacBox
{
    /// <summary>
    /// Refers FlacMethod and data that will be used during encoding.
    /// </summary>
    public class FlacMethodAndDataPair
    {
        public readonly FlacMethod Method;
        public readonly int BitsPerSample;
        public readonly int[] Data;

        public FlacMethodAndDataPair(FlacMethod method, int bitsPerSample, int[] data)
        {
            Method = method;
            Data = data;
            BitsPerSample = bitsPerSample;
        }

        internal void WriteData(FlacBitStreamWriter bitWriter)
        {
            Method.WriteData(bitWriter, BitsPerSample, Data); 
        }
    }
}
