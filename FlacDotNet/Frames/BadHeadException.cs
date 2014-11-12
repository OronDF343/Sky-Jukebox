using System;

namespace FlacDotNet.Frames
{
    internal class BadHeaderException : Exception
    {
        /**
         * Exception constructor.
         */

        public BadHeaderException()
        {
        }

        /**
         * Exception constructor.
         * @param msg   The exception message
         */

        public BadHeaderException(String msg)
            : base(msg)
        {
        }

        /**
         * Exception constructor.
         * @param err   The throwable error
         */

        public BadHeaderException(Exception err)
            : base("BadHeaderException", err)
        {
        }

        /**
         * Exception constructor.
         * @param msg   The exception message
         * @param err   The throwable error
         */

        public BadHeaderException(String msg, Exception err)
            : base(msg, err)
        {
        }
    }
}