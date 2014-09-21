namespace FlacBox
{
    interface IFrameSink
    {
        void StartFrame(long streamPosition, long startSample);
        void EndFrame(long streamPosition, long endSample);
    }

    sealed class NullFrameSink : IFrameSink
    {
        internal static readonly NullFrameSink Instance = new NullFrameSink();

        #region IFrameSink Members

        public void StartFrame(long streamPosition, long startSample)
        {
        }

        public void EndFrame(long streamPosition, long endSample)
        {
        }

        #endregion
    }
}
