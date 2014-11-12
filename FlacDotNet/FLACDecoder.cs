using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FlacDotNet.Frames;
using FlacDotNet.IO;
using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet
{
    public class FLACDecoder : IDisposable
    {
        private const int FRAME_FOOTER_CRC_LEN = 16; // bits
        private static readonly byte[] ID3V2_TAG = new[] {(byte) 'I', (byte) 'D', (byte) '3'};

        private readonly BitInputStream bitStream;
        private readonly ChannelData[] channelData = new ChannelData[Constants.MAX_CHANNELS];
        private readonly FrameListeners frameListeners = new FrameListeners();
        private readonly byte[] headerWarmup = new byte[2]; // contains the sync code and reserved bits
        private readonly PCMProcessors pcmProcessors = new PCMProcessors();
        private int badFrames;
        //private int state;
        private int bitsPerSample;
        private int blockSize; // in samples (per channel)
        private int channelAssignment;
        private int channels;
        private bool eof;
        private Frame frame = new Frame();
        private Stream inputStream;
        private int lastFrameNumber;
        private int outputCapacity;
        private int outputChannels;
        private int sampleRate; // in Hz
        private long samplesDecoded;
        private StreamInfo streamInfo;

        public FLACDecoder(Stream inputStream)
        {
            this.inputStream = inputStream;
            bitStream = new BitInputStream(inputStream);
            //state = DECODER_SEARCH_FOR_METADATA;
            lastFrameNumber = 0;
            samplesDecoded = 0;
            //state = DECODER_SEARCH_FOR_METADATA;
        }

        /**
         * Return the parsed StreamInfo Metadata record.
         * @return  The StreamInfo
         */

        public StreamInfo GetStreamInfo()
        {
            return streamInfo;
        }

        /**
         * Return the ChannelData object.
         * @return  The ChannelData object
         */

        public ChannelData[] GetChannelData()
        {
            return channelData;
        }

        /**
         * Return the input but stream.
         * @return  The bit stream
         */

        public BitInputStream GetBitInputStream()
        {
            return bitStream;
        }

        /**
         * Add a frame listener.
         * @param listener  The frame listener to add
         */

        public void AddFrameListener(IFrameListener listener)
        {
            frameListeners.AddFrameListener(listener);
        }

        /**
         * Remove a frame listener.
         * @param listener  The frame listener to remove
         */

        public void RemoveFrameListener(IFrameListener listener)
        {
            frameListeners.RemoveFrameListener(listener);
        }

        /**
         * Add a PCM processor.
         * @param processor  The processor listener to add
         */

        public void AddPCMProcessor(IPCMProcessor processor)
        {
            pcmProcessors.AddPCMProcessor(processor);
        }

        /**
         * Remove a PCM processor.
         * @param processor  The processor listener to remove
         */

        public void RemovePCMProcessor(IPCMProcessor processor)
        {
            pcmProcessors.RemovePCMProcessor(processor);
        }

        private void CallPCMProcessors(Frame frame)
        {
            ByteData bd = DecodeFrame(frame, null);
            pcmProcessors.ProcessPCM(bd);
        }

        /**
         * Fill the given ByteData object with PCM data from the frame.
         *
         * @param frame the frame to send to the PCM processors
         * @param pcmData the byte data to be filled, or null if it should be allocated
         * @return the ByteData that was filled (may be a new instance from <code>space</code>) 
         */

        public ByteData DecodeFrame(Frame frame, ByteData pcmData)
        {
            // required size of the byte buffer
            int byteSize = frame.Header.BlockSize*channels*((streamInfo.BitsPerSample + 7)/2);
            if (pcmData == null || pcmData.Data.Length < byteSize)
            {
                pcmData = new ByteData(byteSize);
            }
            else
            {
                pcmData.SetLength(0);
            }
            if (streamInfo.BitsPerSample == 8)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < channels; channel++)
                    {
                        pcmData.Append((byte) (channelData[channel].Output[i] + 0x80));
                    }
                }
            }
            else if (streamInfo.BitsPerSample == 16)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < channels; channel++)
                    {
                        var val = (short) (channelData[channel].Output[i]);
                        pcmData.Append((byte) (val & 0xff));
                        pcmData.Append((byte) ((val >> 8) & 0xff));
                    }
                }
            }
            else if (streamInfo.BitsPerSample == 24)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < channels; channel++)
                    {
                        int val = (channelData[channel].Output[i]);
                        pcmData.Append((byte) (val & 0xff));
                        pcmData.Append((byte) ((val >> 8) & 0xff));
                        pcmData.Append((byte) ((val >> 16) & 0xff));
                    }
                }
            }
            return pcmData;
        }

        /**
         * Read the FLAC stream info.
         * @return  The FLAC Stream Info record
         * @throws IOException On read error
         */

        public StreamInfo ReadStreamInfo()
        {
            ReadStreamSync();
            Metadata metadata = ReadNextMetadata();
            if (!(metadata is StreamInfo)) throw new IOException("StreamInfo metadata block missing");
            return (StreamInfo) metadata;
        }

        /**
         * Read an array of metadata blocks.
         * @return  The array of metadata blocks
         * @throws IOException  On read error
         */
        private List<Metadata> metadataList = new List<Metadata>(); 
        public Metadata[] ReadMetadata()
        {
            ReadStreamSync();
            Metadata metadata;
            do
            {
                metadata = ReadNextMetadata();
                metadataList.Add(metadata);
            } while (!metadata.isLast);
            return metadataList.ToArray();
        }

        /**
         * Read an array of metadata blocks.
         * @param currentStreamInfo    The StreamInfo metadata block previously read
         * @return  The array of metadata blocks
         * @throws IOException  On read error
         */

        public Metadata[] ReadMetadata(StreamInfo currentStreamInfo)
        {
            if (currentStreamInfo.isLast) return new Metadata[0];
            var metadataList = new List<Metadata>();
            Metadata metadata;
            do
            {
                metadata = ReadNextMetadata();
                metadataList.Add(metadata);
            } while (!metadata.isLast);
            return metadataList.ToArray();
        }

        /**
         * Decode the FLAC file.
         * @throws IOException  On read error
         */

        private bool _run = true;

        public void StopDecode()
        {
            _run = false;
        }

        public bool Decode()
        {
            _run = true;
            ReadMetadata();
            try
            {
                while (_run)
                {
                    //switch (state) {
                    //case DECODER_SEARCH_FOR_METADATA :
                    //    readStreamSync();
                    //    break;
                    //case DECODER_READ_METADATA :
                    //    Metadata metadata = readNextMetadata();
                    //    if (metadata == null) break;
                    //    break;
                    //case DECODER_SEARCH_FOR_FRAME_SYNC :
                    FindFrameSync();
                    //    break;
                    //case DECODER_READ_FRAME :
                    try
                    {
                        ReadFrame();
                        frameListeners.ProcessFrame(ref frame);
                        CallPCMProcessors(frame);
                    }
                    catch (FrameDecodeException)
                    {
                        badFrames++;
                    }
                    //    break;
                    //case DECODER_END_OF_STREAM :
                    //case DECODER_ABORTED :
                    //    return;
                    //default :
                    //    throw new IOException("Unknown state: " + state);
                    //}
                }
            }
            catch (EndOfStreamException)
            {
                eof = true;
            }
            return _run;
        }

        /**
     * Decode the data frames.
     * @throws IOException  On read error
     */

        public void DecodeFrames()
        {
            //state = DECODER_SEARCH_FOR_FRAME_SYNC;
            try
            {
                while (_run)
                {
                    //switch (state) {
                    //case DECODER_SEARCH_FOR_METADATA :
                    //    readStreamSync();
                    //    break;
                    //case DECODER_READ_METADATA :
                    //    Metadata metadata = readNextMetadata();
                    //    if (metadata == null) break;
                    //    break;
                    //case DECODER_SEARCH_FOR_FRAME_SYNC :
                    FindFrameSync();
                    //    break;
                    //case DECODER_READ_FRAME :
                    try
                    {
                        ReadFrame();
                        frameListeners.ProcessFrame(ref frame);
                        CallPCMProcessors(frame);
                    }
                    catch (FrameDecodeException)
                    {
                        badFrames++;
                    }
                    //    break;
                    //case DECODER_END_OF_STREAM :
                    //case DECODER_ABORTED :
                    //    return;
                    //default :
                    //    throw new IOException("Unknown state: " + state);
                    //}
                }
            }
            catch (EndOfStreamException)
            {
                eof = true;
            }
        }


        /**
         * Read the next data frame.
         * @return  The next frame
         * @throws IOException  on read error
         */

        public Frame ReadNextFrame()
        {
            //boolean got_a_frame;

            try
            {
                while (_run)
                {
                    //switch (state) {
                    //case STREAM_DECODER_SEARCH_FOR_METADATA :
                    //    findMetadata();
                    //    break;
                    //case STREAM_DECODER_READ_METADATA :
                    //    readMetadata(); /* above function sets the status for us */
                    //    break;
                    //case DECODER_SEARCH_FOR_FRAME_SYNC :
                    FindFrameSync(); /* above function sets the status for us */
                    //System.exit(0);
                    //break;
                    //case DECODER_READ_FRAME :
                    try
                    {
                        ReadFrame();
                        return frame;
                    }
                    catch (FrameDecodeException)
                    {
                        badFrames++;
                    }
                    //break;
                    //case DECODER_END_OF_STREAM :
                    //case DECODER_ABORTED :
                    //    return null;
                    //default :
                    //    return null;
                    //}
                }
            }
            catch (EndOfStreamException)
            {
                eof = true;
            }
            return null;
        }

        /**
         * Bytes read.
         * @return  The number of bytes read
         */

        public long GetTotalBytesRead()
        {
            return bitStream.GetTotalBytesRead();
        }


        private void AllocateOutput(int size, int channels)
        {
            if (size <= outputCapacity && channels <= outputChannels) return;

            for (int i = 0; i < Constants.MAX_CHANNELS; i++)
            {
                channelData[i] = null;
            }

            for (int i = 0; i < channels; i++)
            {
                channelData[i] = new ChannelData(size);
            }

            outputCapacity = size;
            outputChannels = channels;
        }

        /**
         * Read the stream sync string.
         * @throws IOException  On read error
         */

        private void ReadStreamSync()
        {
            lock (bitStream)
            {
                int id = 0;
                for (int i = 0; i < 4;)
                {
                    int x = bitStream.ReadRawUInt(8);
                    if (x == Constants.STREAM_SYNC_STRING[i])
                    {
                        i++;
                        id = 0;
                    }
                    else if (x == ID3V2_TAG[id])
                    {
                        id++;
                        i = 0;
                        if (id == 3)
                        {
                            SkipID3V2Tag();
                            id = 0;
                        }
                    }
                    else
                    {
                        throw new IOException("Could not find Stream Sync");
                        //i = 0;
                        //id = 0;
                    }
                }
            }
        }

        /**
         * Read a single metadata record.
         * @return  The next metadata record
         * @throws IOException  on read error
         */

        public Metadata ReadNextMetadata()
        {
            Metadata metadata = null;

            bool isLast = (bitStream.ReadRawUInt(Metadata.STREAM_METADATA_IS_LAST_LEN) != 0);
            int type = bitStream.ReadRawUInt(Metadata.STREAM_METADATA_TYPE_LEN);
            int length = bitStream.ReadRawUInt(Metadata.STREAM_METADATA_LENGTH_LEN);

            if (type == Metadata.METADATA_TYPE_STREAMINFO)
            {
                streamInfo = new StreamInfo(bitStream, length, isLast);
                pcmProcessors.ProcessStreamInfo(ref streamInfo);
                metadata = streamInfo;
            }
            else if (type == Metadata.METADATA_TYPE_SEEKTABLE)
            {
                metadata = new SeekTable(bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_APPLICATION)
            {
                metadata = new Application(bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_PADDING)
            {
                metadata = new Padding(bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_VORBIS_COMMENT)
            {
                metadata = new VorbisComment(bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_CUESHEET)
            {
                metadata = new CueSheet(bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_PICTURE)
            {
                metadata = new Picture(bitStream, length, isLast);
            }
            else
            {
                metadata = new Unknown(bitStream, length, isLast);
            }
            frameListeners.ProcessMetadata(metadata);
            //if (isLast) state = DECODER_SEARCH_FOR_FRAME_SYNC;
            return metadata;
        }
        
        private void SkipID3V2Tag()
        {
            // skip the version and flags bytes 
            int verMajor = bitStream.ReadRawInt(8);
            int verMinor = bitStream.ReadRawInt(8);
            int flags = bitStream.ReadRawInt(8);

            // get the size (in bytes) to skip
            int skip = 0;
            for (int i = 0; i < 4; i++)
            {
                int x = bitStream.ReadRawUInt(8);
                skip <<= 7;
                skip |= (x & 0x7f);
            }

            // skip the rest of the tag
            bitStream.ReadByteBlockAlignedNoCRC(null, skip);
        }

        private void FindFrameSync()
        {
            bool first = true;
            //int cnt=0;

            // If we know the total number of samples in the stream, stop if we've read that many.
            // This will stop us, for example, from wasting time trying to sync on an ID3V1 tag.
            if (streamInfo != null && (streamInfo.TotalSamples != 0))
            {
                if (samplesDecoded >= streamInfo.TotalSamples)
                {
                    //state = DECODER_END_OF_STREAM;
                    return;
                }
            }

            // make sure we're byte aligned
            if (!bitStream.IsConsumedByteAligned())
            {
                bitStream.ReadRawUInt(bitStream.BitsLeftForByteAlignment());
            }

            int x;
            try
            {
                while (_run)
                {
                    x = bitStream.ReadRawUInt(8);
                    if (x == 0xff)
                    {
                        // MAGIC NUMBER for the first 8 frame sync bits
                        headerWarmup[0] = (byte) x;
                        x = bitStream.PeekRawUInt(8);

                        /* we have to check if we just read two 0xff's in a row; the second may actually be the beginning of the sync code */
                        /* else we have to check if the second byte is the end of a sync code */
                        if (x >> 2 == 0x3e)
                        {
                            /* MAGIC NUMBER for the last 6 sync bits */
                            headerWarmup[1] = (byte) bitStream.ReadRawUInt(8);
                            //state = DECODER_READ_FRAME;
                            return;
                        }
                    }
                    if (first)
                    {
                        frameListeners.ProcessError(string.Format("FindSync LOST_SYNC: {0}", x & 0xff));
                        first = false;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                if (!first) frameListeners.ProcessError("FindSync LOST_SYNC: Left over data in file");
                //state = DECODER_END_OF_STREAM;
            }
        }

        /**
         * Read the next data frame.
         * @throws IOException  On read error
         * @throws FrameDecodeException On frame decoding error
         */
        private bool firstFrame = true;
        private long firstFrameOffset;
        public void ReadFrame()
        {
            if (firstFrame)
            {
                firstFrame = false;
                firstFrameOffset = inputStream.Position;
            }
            int channel;
            int i;
            int mid, side, left, right;
            int frameCRC; /* the one we calculate from the input stream */
            //int x;

            /* init the CRC */
            frameCRC = 0;
            frameCRC = CRC16.Update(headerWarmup[0], frameCRC);
            frameCRC = CRC16.Update(headerWarmup[1], frameCRC);
            bitStream.ResetReadCRC16(frameCRC);

            try
            {
                frame.Header = new Header(bitStream, headerWarmup, streamInfo);
            }
            catch (BadHeaderException e)
            {
                frameListeners.ProcessError("Found bad header: " + e);
                throw new FrameDecodeException("Bad Frame Header: " + e);
            }
            //if (state == DECODER_SEARCH_FOR_FRAME_SYNC) return false;
            AllocateOutput(frame.Header.BlockSize, frame.Header.Channels);
            for (channel = 0; channel < frame.Header.Channels; channel++)
            {
                // first figure the correct bits-per-sample of the subframe
                int bps = frame.Header.BitsPerSample;
                switch (frame.Header.ChannelAssignment)
                {
                    case Constants.CHANNEL_ASSIGNMENT_INDEPENDENT:
                        /* no adjustment needed */
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_LEFT_SIDE:
                        if (channel == 1)
                            bps++;
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_RIGHT_SIDE:
                        if (channel == 0)
                            bps++;
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_MID_SIDE:
                        if (channel == 1)
                            bps++;
                        break;
                }
                // now read it
                try
                {
                    ReadSubframe(channel, bps);
                }
                catch (IOException e)
                {
                    frameListeners.ProcessError("ReadSubframe: " + e);
                    throw;
                }
            }
            ReadZeroPadding();

            // Read the frame CRC-16 from the footer and check
            frameCRC = bitStream.GetReadCRC16();
            frame.CRC = bitStream.ReadRawUInt(FRAME_FOOTER_CRC_LEN);
            if ((frameCRC & 0xffff) == frame.CRC)
            {
                /* Undo any special channel coding */
                switch (frame.Header.ChannelAssignment)
                {
                    case Constants.CHANNEL_ASSIGNMENT_INDEPENDENT:
                        /* do nothing */
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_LEFT_SIDE:
                        for (i = 0; i < frame.Header.BlockSize; i++)
                            channelData[1].Output[i] = channelData[0].Output[i] - channelData[1].Output[i];
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_RIGHT_SIDE:
                        for (i = 0; i < frame.Header.BlockSize; i++)
                            channelData[0].Output[i] += channelData[1].Output[i];
                        break;
                    case Constants.CHANNEL_ASSIGNMENT_MID_SIDE:
                        for (i = 0; i < frame.Header.BlockSize; i++)
                        {
                            mid = channelData[0].Output[i];
                            side = channelData[1].Output[i];
                            mid <<= 1;
                            if ((side & 1) != 0) // i.e. if 'side' is odd...
                                mid++;
                            left = mid + side;
                            right = mid - side;
                            channelData[0].Output[i] = left >> 1;
                            channelData[1].Output[i] = right >> 1;
                        }
                        //System.exit(1);
                        break;
                }
            }
            else
            {
                // Bad frame, emit error and zero the output signal
                frameListeners.ProcessError(string.Format("CRC Error: {0} vs {1}", ((frameCRC & 0xffff)),
                                                          ((frame.CRC & 0xffff))));
                for (channel = 0; channel < frame.Header.Channels; channel++)
                {
                    for (int j = 0; j < frame.Header.BlockSize; j++)
                        channelData[channel].Output[j] = 0;
                }
            }

            // put the latest values into the public section of the decoder instance
            channels = frame.Header.Channels;
            channelAssignment = frame.Header.ChannelAssignment;
            bitsPerSample = frame.Header.BitsPerSample;
            sampleRate = frame.Header.SampleRate;
            blockSize = frame.Header.BlockSize;

            //samplesDecoded = frame.header.sampleNumber + frame.header.blockSize;
            samplesDecoded += frame.Header.BlockSize;
            //System.out.println(samplesDecoded+" "+frame.header.sampleNumber + " "+frame.header.blockSize);

            //state = DECODER_SEARCH_FOR_FRAME_SYNC;
            //return;
        }

        public Frame ReadFrame(bool returnnull)
        {
            FindFrameSync();
            //    break;
            //case DECODER_READ_FRAME :
            try
            {
                ReadFrame();
                frameListeners.ProcessFrame(ref frame);
                return frame;
            }
            catch (FrameDecodeException)
            {
                badFrames++;
                return returnnull? null:new Frame();
            }
        }

        private void ReadSubframe(int channel, int bps)
        {
            int x;

            x = bitStream.ReadRawUInt(8); /* MAGIC NUMBER */

            bool haveWastedBits = ((x & 1) != 0);
            x &= 0xfe;

            int wastedBits = 0;
            if (haveWastedBits)
            {
                wastedBits = bitStream.ReadUnaryUnsigned() + 1;
                bps -= wastedBits;
            }

            // Lots of magic numbers here
            if ((x & 0x80) != 0)
            {
                frameListeners.ProcessError(string.Format("ReadSubframe LOST_SYNC: {0}", (x & 0xff)));
                //state = DECODER_SEARCH_FOR_FRAME_SYNC;
                throw new FrameDecodeException(string.Format("ReadSubframe LOST_SYNC: {0}", (x & 0xff)));
                //return true;
            }
            else if (x == 0)
            {
                frame.SubFrames[channel] = new ChannelConstant(bitStream, frame.Header, ref channelData[channel], bps,
                                                               wastedBits);
            }
            else if (x == 2)
            {
                frame.SubFrames[channel] = new ChannelVerbatim(bitStream, frame.Header, ref channelData[channel], bps,
                                                               wastedBits);
            }
            else if (x < 16)
            {
                //state = DECODER_UNPARSEABLE_STREAM;
                throw new FrameDecodeException(string.Format("ReadSubframe Bad Subframe Type: {0}", (x & 0xff)));
            }
            else if (x <= 24)
            {
                //FLACSubframe_Fixed subframe = read_subframe_fixed_(channel, bps, (x >> 1) & 7);
                frame.SubFrames[channel] = new ChannelFixed(bitStream, frame.Header, ref channelData[channel], bps,
                                                            wastedBits, (x >> 1) & 7);
            }
            else if (x < 64)
            {
                //state = DECODER_UNPARSEABLE_STREAM;
                throw new FrameDecodeException(string.Format("ReadSubframe Bad Subframe Type: {0}", (x & 0xff)));
            }
            else
            {
                frame.SubFrames[channel] = new ChannelLPC(bitStream, frame.Header, ref channelData[channel], bps,
                                                          wastedBits,
                                                          ((x >> 1) & 31) + 1);
            }
            if (haveWastedBits)
            {
                int i;
                x = frame.SubFrames[channel].WastedBits;
                for (i = 0; i < frame.Header.BlockSize; i++)
                    channelData[channel].Output[i] <<= x;
            }
        }

        private void ReadZeroPadding()
        {
            if (!bitStream.IsConsumedByteAligned())
            {
                int zero = bitStream.ReadRawUInt(bitStream.BitsLeftForByteAlignment());
                if (zero != 0)
                {
                    frameListeners.ProcessError(string.Format("ZeroPaddingError: {0}", (zero)));
                    //state = DECODER_SEARCH_FOR_FRAME_SYNC;
                    throw new FrameDecodeException(string.Format("ZeroPaddingError: {0}", (zero)));
                }
            }
        }

        public long GetSamplesDecoded()
        {
            return samplesDecoded;
        }

        public int GetBadFrames()
        {
            return badFrames;
        }

        public bool IsEOF()
        {
            return eof;
        }

        public Frame GetLastFrame()
        {
            return frame;
        }

        // Ported from libFlac
        public bool Seek(long target)
        {
            int approxBytesPerFrame;
	        bool firstSeek = true;
            var seekTable = metadataList.Find(meta => meta is SeekTable) as SeekTable;

	        /* use values from stream info if we didn't decode a frame */
		    int channels = this.channels < 1 ? streamInfo.Channels : this.channels;
		    int bps = bitsPerSample < 1 ? streamInfo.BitsPerSample : bitsPerSample;

	        /* we are just guessing here */
	        if(streamInfo.MaxFrameSize > 0)
		        approxBytesPerFrame = (streamInfo.MaxFrameSize + streamInfo.MinFrameSize) / 2 + 1;
	        /*
	         * Check if it's a known fixed-blocksize stream.  Note that though
	         * the spec doesn't allow zeroes in the STREAMINFO block, we may
	         * never get a STREAMINFO block when decoding so the value of
	         * min_blocksize might be zero.
	         */
	        else if(streamInfo.MinBlockSize == streamInfo.MaxBlockSize && streamInfo.MinBlockSize > 0)
		        approxBytesPerFrame = streamInfo.MinBlockSize * channels * bps/8 + 64;
	        else
		        approxBytesPerFrame = 4096 * channels * bps/8 + 64;

	        /*
	         * First, we set an upper and lower bound on where in the
	         * stream we will search.  For now we assume the worst case
	         * scenario, which is our best guess at the beginning of
	         * the first frame and end of the stream.
	         */
	        long lowerBound = firstFrameOffset;
	        long lowerBoundSample = 0;
	        long upperBound = inputStream.Length;
	        long upperBoundSample = streamInfo.TotalSamples > 0 ? streamInfo.TotalSamples : target;

	        /*
	         * Now we refine the bounds if we have a seektable with
	         * suitable points.  Note that according to the spec they
	         * must be ordered by ascending sample number.
	         *
	         * Note: to protect against invalid seek tables we will ignore points
	         * that have frame_samples==0 or sample_number>=total_samples
	         */
	        if(seekTable != default(Metadata))
            {
		        long newLowerBound = lowerBound;
		        long newUpperBound = upperBound;
		        long newLowerBoundSample = lowerBoundSample;
		        long newUpperBoundSample = upperBoundSample;

		        /* find the closest seek point <= target_sample, if it exists */
                var lowerPoint = seekTable.Points.FirstOrDefault(p => p.SampleNumber != long.MinValue &&
                                                                      p.FrameSamples > 0 &&
                                                                      (p.SampleNumber < streamInfo.TotalSamples) &&
                                                                      p.SampleNumber <= target);
		        if(lowerPoint != default(SeekPoint)) 
                { /* i.e. we found a suitable seek point... */
			        newLowerBound = firstFrameOffset + lowerPoint.StreamOffset;
			        newLowerBoundSample = lowerPoint.SampleNumber;
		        }

                /* find the closest seek point > target_sample, if it exists */
                var upperPoint = seekTable.Points.FirstOrDefault(p => p.SampleNumber != long.MinValue &&
                                                                      p.FrameSamples > 0 &&
                                                                      (p.SampleNumber < streamInfo.TotalSamples) &&
                                                                      p.SampleNumber > target);
		        if(upperPoint != default(SeekPoint)) 
                { /* i.e. we found a suitable seek point... */
			        newUpperBound = firstFrameOffset + upperPoint.StreamOffset;
			        newUpperBoundSample = upperPoint.SampleNumber;
		        }

		        /* final protection against unsorted seek tables; keep original values if bogus */
		        if(newUpperBound >= newLowerBound)
                {
			        lowerBound = newLowerBound;
			        upperBound = newUpperBound;
			        lowerBoundSample = newLowerBoundSample;
			        upperBoundSample = newUpperBoundSample;
		        }
		        else
		        {
		            throw new Exception("newUpperBound < newLowerBound");
		        }
	        }

	        /* there are 2 insidious ways that the following equality occurs, which
	         * we need to fix:
	         *  1) total_samples is 0 (unknown) and target_sample is 0
	         *  2) total_samples is 0 (unknown) and target_sample happens to be
	         *     exactly equal to the last seek point in the seek table; this
	         *     means there is no seek point above it, and upper_bound_samples
	         *     remains equal to the estimate (of target_samples) we made above
	         * in either case it does not hurt to move upper_bound_sample up by 1
	         */
	        if(upperBoundSample == lowerBoundSample)
		        upperBoundSample++;

            //wip
            inputStream.Position = target == 0 ? firstFrameOffset : lowerBound;
            //target_sample = target;
	        while(true)
            {
		        /* check if the bounds are still ok */
		        if (lowerBoundSample >= upperBoundSample || lowerBound > upperBound)
			        return false;

		        long pos = lowerBound + (long)((double)(target - lowerBoundSample) / (upperBoundSample - lowerBoundSample) * (upperBound - lowerBound)) - approxBytesPerFrame;

		        if(pos >= upperBound)
			        pos = upperBound - 1;
		        if(pos < lowerBound)
			        pos = lowerBound;

                //wip
                FindFrameSync();
                ReadFrame();
                if (target >= frame.Header.SampleNumber - frame.Header.BlockSize && target <= frame.Header.SampleNumber)
                    break;

		        long thisFrameSample = frame.Header.SampleNumber;
                if (thisFrameSample == -1)
                    return false; //throw

		        if (0 == samplesDecoded || (thisFrameSample + frame.Header.BlockSize >= upperBoundSample && !firstSeek))
                {
			        /* our last move backwards wasn't big enough, try again */
			        approxBytesPerFrame = approxBytesPerFrame != 0 ? approxBytesPerFrame * 2 : 16;
			        continue;
		        }
		        /* allow one seek over upper bound, so we can get a correct upper_bound_sample for streams with unknown total_samples */
		        firstSeek = false;

		        /* make sure we are not seeking in corrupted stream */
		        if (thisFrameSample < lowerBoundSample)
			        return false;

		        /* we need to narrow the search */
		        if(target < thisFrameSample)
                {
			        upperBoundSample = thisFrameSample + frame.Header.BlockSize;
                    upperBound -= bitStream.GETInputBytesUnconsumed();
			        approxBytesPerFrame = (int)(2 * (upperBound - pos) / 3 + 16);
		        }
		        else
                { /* target_sample >= this_frame_sample + this frame's blocksize */
			        lowerBoundSample = thisFrameSample + frame.Header.BlockSize;
                    lowerBound -= bitStream.GETInputBytesUnconsumed();
			        approxBytesPerFrame = (int)(2 * (lowerBound - pos) / 3 + 16);
		        }
	        }
	        return true;
        }

        public void Dispose()
        {
            inputStream.Close();
            inputStream.Dispose();
        }
    }
}