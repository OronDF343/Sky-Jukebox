namespace FlacDotNet.Frames
{
    public class EntropyPartitionedRiceContents
    {
        internal int CapacityByOrder = 0;
        internal int[] Parameters; // The Rice parameters for each context.
        internal int[] RawBits; // Widths for escape-coded partitions.

        /** 
         * The capacity of the parameters and raw_bits arrays specified as an order.
         * i.e. the number of array elements allocated is 2 ^ capacity_by_order.
         */

        /**
         * Ensure enough menory has been allocated.
         * @param maxPartitionOrder The maximum partition order
         */

        public void EnsureSize(int maxPartitionOrder)
        {
            if (CapacityByOrder >= maxPartitionOrder) return;
            Parameters = new int[(1 << maxPartitionOrder)];
            RawBits = new int[(1 << maxPartitionOrder)];
            CapacityByOrder = maxPartitionOrder;
        }
    }
}