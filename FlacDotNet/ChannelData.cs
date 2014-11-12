using FlacDotNet.Frames;

namespace FlacDotNet
{
    public class ChannelData
    {
        public ChannelData(int size)
        {
            Output = new int[size];
            Residual = new int[size];
            PartitionedRiceContents = new EntropyPartitionedRiceContents();
        }

        public int[] Output { get; set; }

        public int[] Residual { get; set; }

        public EntropyPartitionedRiceContents PartitionedRiceContents { get; private set; }
    }
}