using System;
using FlacDotNet.IO;

namespace FlacDotNet.Frames
{
    internal class EntropyPartitionedRice : EntropyCodingMethod
    {
        private const int ENTROPY_CODING_METHOD_PARTITIONED_RICE_PARAMETER_LEN = 4; /* bits */
        private const int ENTROPY_CODING_METHOD_PARTITIONED_RICE_RAW_LEN = 5; /* bits */
        private const int ENTROPY_CODING_METHOD_PARTITIONED_RICE_ESCAPE_PARAMETER = 15;

        internal EntropyPartitionedRiceContents Contents; // The context's Rice parameters and/or raw bits.
        internal int Order; // The partition order, i.e. # of contexts = 2 ^ order.

        internal void ReadResidual(BitInputStream inputStream, int predictorOrder, int partitionOrder, Header header,
                                   ref int[] residual)
        {
            //System.out.println("readREsidual Pred="+predictorOrder+" part="+partitionOrder);
            int sample = 0;
            int partitions = 1 << partitionOrder;
            int partitionSamples = partitionOrder > 0
                                       ? header.BlockSize >> partitionOrder
                                       : header.BlockSize - predictorOrder;
            Contents.EnsureSize(Math.Max(6, partitionOrder));
            Contents.Parameters = new int[partitions];

            for (int partition = 0; partition < partitions; partition++)
            {
                int riceParameter = inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_PARTITIONED_RICE_PARAMETER_LEN);
                Contents.Parameters[partition] = riceParameter;
                if (riceParameter < ENTROPY_CODING_METHOD_PARTITIONED_RICE_ESCAPE_PARAMETER)
                {
                    int u = (partitionOrder == 0 || partition > 0)
                                ? partitionSamples
                                : partitionSamples - predictorOrder;
                    inputStream.ReadRiceSignedBlock(ref residual, sample, u, riceParameter);
                    sample += u;
                }
                else
                {
                    riceParameter = inputStream.ReadRawUInt(ENTROPY_CODING_METHOD_PARTITIONED_RICE_RAW_LEN);
                    Contents.RawBits[partition] = riceParameter;
                    for (int u = (partitionOrder == 0 || partition > 0) ? 0 : predictorOrder;
                         u < partitionSamples;
                         u++, sample++)
                    {
                        residual[sample] = inputStream.ReadRawInt(riceParameter);
                    }
                }
            }
        }
    }
}