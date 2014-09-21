using System;

namespace FlacBox
{
    /// <summary>
    /// Base FlacBox exception class.
    /// </summary>
    public class FlacException : Exception
    {
        public FlacException(string message)
            : base(message)
        {
        }
    }
}
