using System.IO;

namespace FlacDotNet.Util
{
    internal class LittleEndianDataOutput : DataOutput
    {
        public LittleEndianDataOutput(Stream stream) : base(stream)
        {
        }
    }
}