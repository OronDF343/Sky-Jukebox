using System;
using FlacDotNet.IO;

namespace FlacDotNet.Frames
{
    internal class ChannelConstant : Channel
    {
        private readonly int _value;

        public ChannelConstant(BitInputStream inputStream, Header header, ref ChannelData channelData, int bps,
                               int wastedBits)
            : base(header, wastedBits)
        {
            _value = inputStream.ReadRawInt(bps);

            // decode the subframe
            for (int i = 0; i < header.BlockSize; i++) channelData.Output[i] = _value;
        }

        public override String ToString()
        {
            return "ChannelConstant: Value=" + _value + " WastedBits=" + WastedBits;
        }
    }
}