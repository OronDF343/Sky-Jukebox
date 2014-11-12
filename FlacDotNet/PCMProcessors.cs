using System.Collections.Generic;
using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet
{
    internal class PCMProcessors : IPCMProcessor
    {
        private readonly List<IPCMProcessor> _pcmProcessors = new List<IPCMProcessor>();

        #region IPCMProcessor Members

        public void ProcessStreamInfo(ref StreamInfo streamInfo)
        {
            lock (_pcmProcessors)
            {
                List<IPCMProcessor>.Enumerator it = _pcmProcessors.GetEnumerator();
                while (it.MoveNext())
                {
                    IPCMProcessor processor = it.Current;
                    if (processor != null) processor.ProcessStreamInfo(ref streamInfo);
                }
            }
        }

        public void ProcessPCM(ByteData pcm)
        {
            lock (_pcmProcessors)
            {
                List<IPCMProcessor>.Enumerator it = _pcmProcessors.GetEnumerator();
                while (it.MoveNext())
                {
                    IPCMProcessor processor = it.Current;
                    if (processor != null) processor.ProcessPCM(pcm);
                }
            }
        }

        #endregion

        public void AddPCMProcessor(IPCMProcessor processor)
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

        public void RemovePCMProcessor(IPCMProcessor processor)
        {
            lock (_pcmProcessors)
            {
                if (_pcmProcessors.Contains(processor))
                    _pcmProcessors.Remove(processor);
            }
        }
    }
}