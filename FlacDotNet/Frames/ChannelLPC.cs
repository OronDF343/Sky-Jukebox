using System;
using System.IO;
using System.Text;
using FlacDotNet.IO;
using FlacDotNet.Util;

namespace FlacDotNet.Frames
{
    internal class ChannelLPC : Channel
    {
        private const int SUBFRAME_LPC_QLP_COEFF_PRECISION_LEN = 4; /* bits */
        private const int SUBFRAME_LPC_QLP_SHIFT_LEN = 5; /* bits */
        private const int MAX_LPC_ORDER = 32;

        private readonly EntropyCodingMethod _entropyCodingMethod; // The residual coding method.
        private readonly int[] _qlpCoeff = new int[MAX_LPC_ORDER]; // FIR filter coefficients.

        private readonly int[] _warmup = new int[MAX_LPC_ORDER];
        internal int Order; // The FIR order.
        internal int QlpCoeffPrecision; // Quantized FIR filter coefficient precision in bits.
        internal int QuantizationLevel; // The qlp coeff shift needed.
        // Warmup samples to prime the predictor, length == order.

        internal int[] Residual; // The residual signal, length == (blocksize minus order) samples.

        /**
         * The constructor.
         * @param is            The InputBitStream
         * @param header        The FLAC Frame Header
         * @param channelData   The decoded channel data (output)
         * @param bps           The bits-per-second
         * @param wastedBits    The bits waisted in the frame
         * @param order         The predicate order
         * @throws IOException  Thrown if error reading from the InputBitStream
         */

        public ChannelLPC(BitInputStream inputStream, Header header, ref ChannelData channelData, int bps,
                          int wastedBits,
                          int order)
            : base(header, wastedBits)
        {
            Residual = channelData.Residual;
            Order = order;

            // read warm-up samples
            //System.out.println("Order="+order);
            for (int u = 0; u < order; u++)
            {
                _warmup[u] = inputStream.ReadRawInt(bps);
            }
            //for (int i = 0; i < order; i++) System.out.println("Warm "+i+" "+warmup[i]);

            // read qlp coeff precision
            int u32 = inputStream.ReadRawUInt(SUBFRAME_LPC_QLP_COEFF_PRECISION_LEN);
            if (u32 == (1 << SUBFRAME_LPC_QLP_COEFF_PRECISION_LEN) - 1)
            {
                throw new IOException("STREAM_DECODER_ERROR_STATUS_LOST_SYNC");
            }
            QlpCoeffPrecision = u32 + 1;
            //System.out.println("qlpCoeffPrecision="+qlpCoeffPrecision);

            // read qlp shift
            QuantizationLevel = inputStream.ReadRawInt(SUBFRAME_LPC_QLP_SHIFT_LEN);
            //System.out.println("quantizationLevel="+quantizationLevel);

            // read quantized lp coefficiencts
            for (int u = 0; u < order; u++)
            {
                _qlpCoeff[u] = inputStream.ReadRawInt(QlpCoeffPrecision);
            }

            // read entropy coding method info
            int codingType = inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_TYPE_LEN);
            //System.out.println("codingType="+codingType);
            switch (codingType)
            {
                case ENTROPY_CODING_METHOD_PARTITIONED_RICE:
                    _entropyCodingMethod = new EntropyPartitionedRice();
                    ((EntropyPartitionedRice) _entropyCodingMethod).Order =
                        inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_PARTITIONED_RICE_ORDER_LEN);
                    ((EntropyPartitionedRice) _entropyCodingMethod).Contents = channelData.PartitionedRiceContents;
                    break;
                default:
                    throw new IOException("STREAM_DECODER_UNPARSEABLE_STREAM");
            }

            // read residual
            var entropyPartitionedRice = _entropyCodingMethod as EntropyPartitionedRice;
            int[] residual = channelData.Residual;
            entropyPartitionedRice.ReadResidual(inputStream,
                                                order,
                                                entropyPartitionedRice.Order,
                                                header, ref residual);
            channelData.Residual = residual;
            Array.Copy(_warmup, 0, channelData.Output, 0, order);
            if (bps + QlpCoeffPrecision + order.Ilog2() <= 32)
            {
                if (bps <= 16 && QlpCoeffPrecision <= 16)
                {
                    int[] output = channelData.Output;
                    LPCPredictor.RestoreSignal(channelData.Residual, header.BlockSize - order, _qlpCoeff, order,
                                               QuantizationLevel, ref output, order);
                    channelData.Output = output;
                }
                else
                {
                    int[] output = channelData.Output;
                    LPCPredictor.RestoreSignal(channelData.Residual, header.BlockSize - order, _qlpCoeff, order,
                                               QuantizationLevel, ref output, order);
                    channelData.Output = output;
                }
            }
            else
            {
                int[] output = channelData.Output;
                LPCPredictor.RestoreSignalWide(channelData.Residual, header.BlockSize - order, _qlpCoeff, order,
                                               QuantizationLevel, ref output, order);
                channelData.Output = output;
            }
        }

        /**
         * @see java.lang.Object#toString()
         */

        public override String ToString()
        {
            var sb = new StringBuilder("ChannelLPC: Order=" + Order + " WastedBits=" + WastedBits);
            sb.Append(" qlpCoeffPrecision=" + QlpCoeffPrecision + " quantizationLevel=" + QuantizationLevel);
            sb.Append("\n\t\tqlpCoeff: ");
            for (int i = 0; i < Order; i++) sb.Append(_qlpCoeff[i] + " ");
            sb.Append("\n\t\tWarmup: ");
            for (int i = 0; i < Order; i++) sb.Append(_warmup[i] + " ");
            sb.Append("\n\t\tParameter: ");
            for (int i = 0; i < (1 << ((EntropyPartitionedRice) _entropyCodingMethod).Order); i++)
                sb.Append(((EntropyPartitionedRice) _entropyCodingMethod).Contents.Parameters[i] + " ");
            //sb.append("\n\t\tResidual: ");
            //for (int i = 0; i < header.blockSize; i++) sb.append(residual[i] + " ");
            return sb.ToString();
        }
    }
}