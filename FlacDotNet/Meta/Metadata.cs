namespace FlacDotNet.Meta
{
    public abstract class Metadata
    {
        /** StreamInfo Metatdata type. */
        public const int METADATA_TYPE_STREAMINFO = 0;

        /** Padding Metatdata type. */
        public const int METADATA_TYPE_PADDING = 1;

        /** Application Metatdata type. */
        public const int METADATA_TYPE_APPLICATION = 2;

        /** SeekTable Metatdata type. */
        public const int METADATA_TYPE_SEEKTABLE = 3;

        /** VorbisComment Metatdata type. */
        public const int METADATA_TYPE_VORBIS_COMMENT = 4;

        /** CueSheet Metatdata type. */
        public const int METADATA_TYPE_CUESHEET = 5;

        /** Picture Metatdata type. */
        public const int METADATA_TYPE_PICTURE = 6;

        /** Metadata IsLast field length. */
        public const int STREAM_METADATA_IS_LAST_LEN = 1; // bits

        /** Metadata type field length. */
        public const int STREAM_METADATA_TYPE_LEN = 7; // bits

        /** Metadata length field length. */
        public const int STREAM_METADATA_LENGTH_LEN = 24; // bits

        /**
         * Constructir.
         * @param isLast    True if last Metadata block
         */

        protected Metadata(bool isLast)
        {
            this.isLast = isLast;
        }

        public bool isLast { get; private set; }
    }
}