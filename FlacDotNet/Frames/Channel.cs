namespace FlacDotNet.Frames
{
    public class Channel
    {
        /** Partisioned Rice Encoding Method. */
        public const int ENTROPY_CODING_METHOD_PARTITIONED_RICE = 0;

        /** The size of the encoding method field (in bits). */
        public const int ENTROPY_CODING_METHOD_TYPE_LEN = 2;

        /** The size of the Rice Order field (in bits). */
        public const int ENTROPY_CODING_METHOD_PARTITIONED_RICE_ORDER_LEN = 4;

        /** The FLAC Frame Header. */
        protected Header header;

        /** The number of waisted bits in the frame. */

        /**
     * The constructor.
     * @param header        The FLAC Frame Header
     * @param wastedBits    The number of waisted bits in the frame
     */

        protected Channel(Header header, int wastedBits)
        {
            this.header = header;
            WastedBits = wastedBits;
        }

        public int WastedBits { get; protected set; }
    }
}