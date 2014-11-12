using System;

namespace FlacDotNet
{
    internal class FrameDecodeException : Exception
    {
        public FrameDecodeException()
        {
        }

        public FrameDecodeException(String msg) : base(msg)
        {
        }

        public FrameDecodeException(Exception err)
            : base("FrameDecodeException", err)
        {
        }

        public FrameDecodeException(String msg, Exception err) : base(msg, err)
        {
        }
    }
}