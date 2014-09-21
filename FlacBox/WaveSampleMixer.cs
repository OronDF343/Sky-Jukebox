using System.Collections.Generic;

namespace FlacBox
{
    /// <summary>
    /// Helps to mix/interleave samples.
    /// </summary>
    abstract class WaveSampleMixer
    {
        internal abstract IEnumerable<int> MixSamples(FlacReader reader);        
    }

    class AsIsWaveSampleMixer : WaveSampleMixer
    {
        internal override IEnumerable<int> MixSamples(FlacReader reader)
        {
            var blockSize = reader.BlockSize;
            var channelCount = reader.Streaminfo.ChannelsCount;

            var channelData = new List<int[]>(channelCount);
            while (reader.Read() && reader.RecordType == FlacRecordType.Subframe)
            {
                var data = reader.ReadSubframeValues();
                channelData.Add(data);
            }

            for (var i = 0; i < blockSize; i++)
            {
                for (var j = 0; j < channelCount; j++)
                {
                    yield return channelData[j][i];
                }
            }
        }
    }

    class RightSideWaveSampleMixer : WaveSampleMixer
    {
        internal override IEnumerable<int> MixSamples(FlacReader reader)
        {
            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Side channel expected");
            var side = reader.ReadSubframeValues();

            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Right channel expected");
            var right = reader.ReadSubframeValues();

            for (var i = 0; i < right.Length; i++)
            {
                yield return right[i] + side[i];
                yield return right[i];
            }

            reader.Read();
        }
    }


    class LeftSideWaveSampleMixer : WaveSampleMixer
    {
        internal override IEnumerable<int> MixSamples(FlacReader reader)
        {
            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Right channel expected");
            var left = reader.ReadSubframeValues();

            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Side channel expected");
            var side = reader.ReadSubframeValues();

            for (var i = 0; i < left.Length; i++)
            {
                yield return left[i];
                yield return left[i] - side[i];
            }

            reader.Read();
        }
    }

    class AverageWaveSampleMixer : WaveSampleMixer
    {
        internal override IEnumerable<int> MixSamples(FlacReader reader)
        {
            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Mid channel expected");
            var mid = reader.ReadSubframeValues();

            if (!reader.Read() || reader.RecordType != FlacRecordType.Subframe)
                throw new FlacException("Side channel expected");
            var side = reader.ReadSubframeValues();

            for (var i = 0; i < mid.Length; i++)
            {
                var right = mid[i] - (side[i] >> 1);
                yield return right + side[i];
                yield return right;
            }

            reader.Read();
        }
    }
}
