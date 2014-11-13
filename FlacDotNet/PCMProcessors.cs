using System.Collections.Generic;
using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet
{
    internal class PcmProcessors : IPcmProcessor
    {
        private readonly List<IPcmProcessor> _pcmProcessors = new List<IPcmProcessor>();

        #region IPcmProcessor Members

        public void ProcessStreamInfo(ref StreamInfo streamInfo)
        {
            lock (_pcmProcessors)
            {
                List<IPcmProcessor>.Enumerator it = _pcmProcessors.GetEnumerator();
                while (it.MoveNext())
                {
                    IPcmProcessor processor = it.Current;
                    if (processor != null) processor.ProcessStreamInfo(ref streamInfo);
                }
            }
        }

        public void ProcessPcm(ByteData pcm)
        {
            lock (_pcmProcessors)
            {
                List<IPcmProcessor>.Enumerator it = _pcmProcessors.GetEnumerator();
                while (it.MoveNext())
                {
                    IPcmProcessor processor = it.Current;
                    if (processor != null) processor.ProcessPcm(pcm);
                }
            }
        }

        #endregion

        public void AddPcmProcessor(IPcmProcessor processor)
        {
            lock (_pcmProcessors)
            {
                if (!_pcmProcessors.Contains(processor))
                    _pcmProcessors.Add(processor);
            }
        }

        /**
         * Remove a PCM processor.
         * @param processor  The processor listener to remove
         */

        public void RemovePcmProcessor(IPcmProcessor processor)
        {
            lock (_pcmProcessors)
            {
                if (_pcmProcessors.Contains(processor))
                    _pcmProcessors.Remove(processor);
            }
        }
    }
}