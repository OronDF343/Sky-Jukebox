using System.Collections.Generic;
using FlacDotNet.Frames;
using FlacDotNet.Meta;

namespace FlacDotNet
{
    internal class FrameListeners : IFrameListener
    {
        private readonly List<IFrameListener> _frameListeners = new List<IFrameListener>();

        #region IFrameListener Members

        public void ProcessMetadata(Metadata metadata)
        {
            lock (_frameListeners)
            {
                List<IFrameListener>.Enumerator it = _frameListeners.GetEnumerator();
                while (it.MoveNext())
                {
                    IFrameListener listener = it.Current;
                    if (listener != null) listener.ProcessMetadata(metadata);
                }
            }
        }

        public void ProcessFrame(ref Frame frame)
        {
            lock (_frameListeners)
            {
                List<IFrameListener>.Enumerator it = _frameListeners.GetEnumerator();
                while (it.MoveNext())
                {
                    IFrameListener listener = it.Current;
                    if (listener != null) listener.ProcessFrame(ref frame);
                }
            }
        }

        public void ProcessError(string msg)
        {
            lock (_frameListeners)
            {
                List<IFrameListener>.Enumerator it = _frameListeners.GetEnumerator();
                while (it.MoveNext())
                {
                    IFrameListener listener = it.Current;
                    if (listener != null) listener.ProcessError(msg);
                }
            }
        }

        #endregion

        public void AddFrameListener(IFrameListener listener)
        {
            lock (_frameListeners)
            {
                if (!_frameListeners.Contains(listener))
                    _frameListeners.Add(listener);
            }
        }

        public void RemoveFrameListener(IFrameListener listener)
        {
            lock (_frameListeners)
            {
                if (_frameListeners.Contains(listener))
                    _frameListeners.Remove(listener);
            }
        }
    }
}