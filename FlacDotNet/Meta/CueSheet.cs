using FlacDotNet.IO;

namespace FlacDotNet.Meta
{
    internal class CueSheet : Metadata
    {
        private const int CUESHEET_MEDIA_CATALOG_NUMBER_LEN = 128*8; // bits
        private const int CUESHEET_LEAD_IN_LEN = 64; // bits
        private const int CUESHEET_IS_CD_LEN = 1; // bit
        private const int CUESHEET_RESERVED_LEN = 7 + 258*8; // bits
        private const int CUESHEET_NUM_TRACKS_LEN = 8; // bits

        /** 
         * Media catalog number.
         * in ASCII printable characters 0x20-0x7e.  In
         * general, the media catalog number may be 0 to 128 bytes long; any
         * unused characters should be right-padded with NUL characters.
         */
        protected bool isCD = false; // true if CUESHEET corresponds to a Compact Disc, else false
        protected long leadIn = 0; // The number of lead-in samples.
        protected byte[] mediaCatalogNumber = new byte[129];
        protected int numTracks = 0; // The number of tracks.
        protected CueTrack[] tracks; // NULL if num_tracks == 0, else pointer to array of tracks.

        /**
         * The constructor.
         * @param is                The InputBitStream
         * @param length            Length of the record
         * @param isLast            True if this is the last Metadata block in the chain
         * @throws IOException      Thrown if error reading from InputBitStream
         */

        public CueSheet(BitInputStream inputStream, int length, bool isLast)
            : base(isLast)
        {
            inputStream.ReadByteBlockAlignedNoCRC(mediaCatalogNumber, CUESHEET_MEDIA_CATALOG_NUMBER_LEN/8);
            leadIn = inputStream.ReadRawULong(CUESHEET_LEAD_IN_LEN);
            isCD = (inputStream.ReadRawUInt(CUESHEET_IS_CD_LEN) != 0);
            inputStream.SkipBitsNoCRC(CUESHEET_RESERVED_LEN);
            numTracks = inputStream.ReadRawUInt(CUESHEET_NUM_TRACKS_LEN);

            if (numTracks > 0)
            {
                tracks = new CueTrack[numTracks];
                for (int i = 0; i < numTracks; i++)
                {
                    tracks[i] = new CueTrack(inputStream);
                }
            }
        }

        /**
         * Verifys the Cue Sheet.
         * @param checkCdDaSubset   True for check CD subset
         * @throws Violation        Thrown if invalid Cue Sheet
         */

        private void IsLegal(bool checkCdDaSubset)
        {
            if (checkCdDaSubset)
            {
                if (leadIn < 2*44100)
                {
                    throw new Violation("CD-DA cue sheet must have a lead-in length of at least 2 seconds");
                }
                if (leadIn%588 != 0)
                {
                    throw new Violation("CD-DA cue sheet lead-in length must be evenly divisible by 588 samples");
                }
            }

            if (numTracks == 0)
            {
                throw new Violation("cue sheet must have at least one track (the lead-out)");
            }

            if (checkCdDaSubset && tracks[numTracks - 1].Number != 170)
            {
                throw new Violation("CD-DA cue sheet must have a lead-out track number 170 (0xAA)");
            }

            for (int i = 0; i < numTracks; i++)
            {
                if (tracks[i].Number == 0)
                {
                    throw new Violation("cue sheet may not have a track number 0");
                }

                if (checkCdDaSubset)
                {
                    if (!((tracks[i].Number >= 1 && tracks[i].Number <= 99)
                          || tracks[i].Number == 170))
                    {
                        throw new Violation("CD-DA cue sheet track number must be 1-99 or 170");
                    }
                }

                if (checkCdDaSubset && tracks[i].Offset%588 != 0)
                {
                    throw new Violation("CD-DA cue sheet track offset must be evenly divisible by 588 samples");
                }

                if (i < numTracks - 1)
                {
                    if (tracks[i].NumIndices == 0)
                    {
                        throw new Violation("cue sheet track must have at least one index point");
                    }

                    if (tracks[i].Indices[0].Number > 1)
                    {
                        throw new Violation("cue sheet track's first index number must be 0 or 1");
                    }
                }

                for (int j = 0; j < tracks[i].NumIndices; j++)
                {
                    if (checkCdDaSubset && tracks[i].Indices[j].Offset%588 != 0)
                    {
                        throw new Violation("CD-DA cue sheet track index offset must be evenly divisible by 588 samples");
                    }

                    if (j > 0)
                    {
                        if (tracks[i].Indices[j].Number != tracks[i].Indices[j - 1].Number + 1)
                        {
                            throw new Violation("cue sheet track index numbers must increase by 1");
                        }
                    }
                }
            }
        }
    }
}