using System;
using System.IO;
using System.Text;
using FlacDotNet.IO;

namespace FlacDotNet.Frames
{
    internal class ChannelFixed : Channel
    {
        private const int MAX_FIXED_ORDER = 4;

        private readonly EntropyCodingMethod _entropyCodingMethod; // The residual coding method.

        private readonly int[] _warmup = new int[MAX_FIXED_ORDER];
                               // Warmup samples to prime the predictor, length == order.

        internal int Order; // The polynomial order.

        internal int[] Residual; // The residual signal, length == (blocksize minus order) samples.

        public ChannelFixed(BitInputStream inputStream, Header header, ref ChannelData channelData, int bps,
                            int wastedBits,
                            int order)
            : base(header, wastedBits)
        {
            Residual = channelData.Residual;
            Order = order;

            // read warm-up samples
            for (int u = 0; u < order; u++)
            {
                _warmup[u] = inputStream.ReadRawInt(bps);
            }

            // read entropy coding method info
            int type = inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_TYPE_LEN);
            switch (type)
            {
                case ENTROPY_CODING_METHOD_PARTITIONED_RICE:
                    int u32 = inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_PARTITIONED_RICE_ORDER_LEN);
                    var pr = new EntropyPartitionedRice();
                    _entropyCodingMethod = pr;
                    pr.Order = u32;
                    pr.Contents = channelData.PartitionedRiceContents;
                    int[] residual = channelData.Residual;
                    pr.ReadResidual(inputStream, order, pr.Order, header, ref residual);
                    channelData.Residual = residual;
                    break;
                default:
                    throw new IOException("STREAM_DECODER_UNPARSEABLE_STREAM");
            }

            // decode the subframe
            Array.Copy(_warmup, 0, channelData.Output, 0, order);
            int[] output = channelData.Output;
            FixedPredictor.RestoreSignal(Residual, header.BlockSize - order, order, ref output, order);
            channelData.Output = output;
        }

        public override String ToString()
        {
            var sb =
                new StringBuilder("FLACSubframe_Fixed: Order=" + Order + " PartitionOrder=" +
                                  ((EntropyPartitionedRice) _entropyCodingMethod).Order + " WastedBits=" + WastedBits);
            for (int i = 0; i < Order; i++) sb.Append(" warmup[" + i + "]=" + _warmup[i]);
            return sb.ToString();
        }
    }
}