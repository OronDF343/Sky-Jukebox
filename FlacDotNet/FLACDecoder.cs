using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlacDotNet.Frames;
using FlacDotNet.IO;
using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet
{
    public class FlacDecoder : IDisposable
    {
        private const int FrameFooterCrcLen = 16; // bits
        private static readonly byte[] Id3V2Tag = {(byte) 'I', (byte) 'D', (byte) '3'};

        private readonly BitInputStream _bitStream;
        private readonly ChannelData[] _channelData = new ChannelData[Constants.MaxChannels];
        private readonly FrameListeners _frameListeners = new FrameListeners();
        private readonly byte[] _headerWarmup = new byte[2]; // contains the sync code and reserved bits
        private readonly PcmProcessors _pcmProcessors = new PcmProcessors();
        private int _badFrames;
        //private int state;
        private int _bitsPerSample;
        private int _blockSize; // in samples (per channel)
        private int _channelAssignment;
        private int _channels;
        private bool _eof;
        private Frame _frame = new Frame();
        private readonly Stream _inputStream;
        private int _lastFrameNumber;
        private int _outputCapacity;
        private int _outputChannels;
        private int _sampleRate; // in Hz
        private long _samplesDecoded;
        private StreamInfo _streamInfo;

        public FlacDecoder(Stream inputStream)
        {
            _inputStream = inputStream;
            _bitStream = new BitInputStream(inputStream);
            //state = DECODER_SEARCH_FOR_METADATA;
            _lastFrameNumber = 0;
            _samplesDecoded = 0;
            //state = DECODER_SEARCH_FOR_METADATA;
        }

        /**
         * Return the parsed StreamInfo Metadata record.
         * @return  The StreamInfo
         */

        public StreamInfo GetStreamInfo()
        {
            return _streamInfo;
        }

        /**
         * Return the ChannelData object.
         * @return  The ChannelData object
         */

        public ChannelData[] GetChannelData()
        {
            return _channelData;
        }

        /**
         * Return the input but stream.
         * @return  The bit stream
         */

        public BitInputStream GetBitInputStream()
        {
            return _bitStream;
        }

        /**
         * Add a frame listener.
         * @param listener  The frame listener to add
         */

        public void AddFrameListener(IFrameListener listener)
        {
            _frameListeners.AddFrameListener(listener);
        }

        /**
         * Remove a frame listener.
         * @param listener  The frame listener to remove
         */

        public void RemoveFrameListener(IFrameListener listener)
        {
            _frameListeners.RemoveFrameListener(listener);
        }

        /**
         * Add a PCM processor.
         * @param processor  The processor listener to add
         */

        public void AddPcmProcessor(IPcmProcessor processor)
        {
            _pcmProcessors.AddPcmProcessor(processor);
        }

        /**
         * Remove a PCM processor.
         * @param processor  The processor listener to remove
         */

        public void RemovePcmProcessor(IPcmProcessor processor)
        {
            _pcmProcessors.RemovePcmProcessor(processor);
        }

        private void CallPcmProcessors(Frame frame)
        {
            ByteData bd = DecodeFrame(frame, null);
            _pcmProcessors.ProcessPcm(bd);
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
            int byteSize = frame.Header.BlockSize*_channels*((_streamInfo.BitsPerSample + 7)/2);
            if (pcmData == null || pcmData.Data.Length < byteSize)
            {
                pcmData = new ByteData(byteSize);
            }
            else
            {
                pcmData.SetLength(0);
            }
            if (_streamInfo.BitsPerSample == 8)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < _channels; channel++)
                    {
                        pcmData.Append((byte) (_channelData[channel].Output[i] + 0x80));
                    }
                }
            }
            else if (_streamInfo.BitsPerSample == 16)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < _channels; channel++)
                    {
                        var val = (short) (_channelData[channel].Output[i]);
                        pcmData.Append((byte) (val & 0xff));
                        pcmData.Append((byte) ((val >> 8) & 0xff));
                    }
                }
            }
            else if (_streamInfo.BitsPerSample == 24)
            {
                for (int i = 0; i < frame.Header.BlockSize; i++)
                {
                    for (int channel = 0; channel < _channels; channel++)
                    {
                        int val = (_channelData[channel].Output[i]);
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
        private readonly List<Metadata> _metadataList = new List<Metadata>(); 
        public Metadata[] ReadMetadata()
        {
            ReadStreamSync();
            Metadata metadata;
            do
            {
                metadata = ReadNextMetadata();
                _metadataList.Add(metadata);
            } while (!metadata.isLast);
            return _metadataList.ToArray();
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
                        _frameListeners.ProcessFrame(ref _frame);
                        CallPcmProcessors(_frame);
                    }
                    catch (FrameDecodeException)
                    {
                        _badFrames++;
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
                _eof = true;
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
                        _frameListeners.ProcessFrame(ref _frame);
                        CallPcmProcessors(_frame);
                    }
                    catch (FrameDecodeException)
                    {
                        _badFrames++;
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
                _eof = true;
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
                        return _frame;
                    }
                    catch (FrameDecodeException)
                    {
                        _badFrames++;
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
                _eof = true;
            }
            return null;
        }

        /**
         * Bytes read.
         * @return  The number of bytes read
         */

        public long GetTotalBytesRead()
        {
            return _bitStream.GetTotalBytesRead();
        }


        private void AllocateOutput(int size, int channels)
        {
            if (size <= _outputCapacity && channels <= _outputChannels) return;

            for (int i = 0; i < Constants.MaxChannels; i++)
            {
                _channelData[i] = null;
            }

            for (int i = 0; i < channels; i++)
            {
                _channelData[i] = new ChannelData(size);
            }

            _outputCapacity = size;
            _outputChannels = channels;
        }

        /**
         * Read the stream sync string.
         * @throws IOException  On read error
         */

        private void ReadStreamSync()
        {
            lock (_bitStream)
            {
                int id = 0;
                for (int i = 0; i < 4;)
                {
                    int x = _bitStream.ReadRawUInt(8);
                    if (x == Constants.StreamSyncString[i])
                    {
                        i++;
                        id = 0;
                    }
                    else if (x == Id3V2Tag[id])
                    {
                        id++;
                        i = 0;
                        if (id == 3)
                        {
                            SkipId3V2Tag();
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

            bool isLast = (_bitStream.ReadRawUInt(Metadata.STREAM_METADATA_IS_LAST_LEN) != 0);
            int type = _bitStream.ReadRawUInt(Metadata.STREAM_METADATA_TYPE_LEN);
            int length = _bitStream.ReadRawUInt(Metadata.STREAM_METADATA_LENGTH_LEN);

            if (type == Metadata.METADATA_TYPE_STREAMINFO)
            {
                _streamInfo = new StreamInfo(_bitStream, length, isLast);
                _pcmProcessors.ProcessStreamInfo(ref _streamInfo);
                metadata = _streamInfo;
            }
            else if (type == Metadata.METADATA_TYPE_SEEKTABLE)
            {
                metadata = new SeekTable(_bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_APPLICATION)
            {
                metadata = new Application(_bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_PADDING)
            {
                metadata = new Padding(_bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_VORBIS_COMMENT)
            {
                metadata = new VorbisComment(_bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_CUESHEET)
            {
                metadata = new CueSheet(_bitStream, length, isLast);
            }
            else if (type == Metadata.METADATA_TYPE_PICTURE)
            {
                metadata = new Picture(_bitStream, length, isLast);
            }
            else
            {
                metadata = new Unknown(_bitStream, length, isLast);
            }
            _frameListeners.ProcessMetadata(metadata);
            //if (isLast) state = DECODER_SEARCH_FOR_FRAME_SYNC;
            return metadata;
        }
        
        private void SkipId3V2Tag()
        {
            // skip the version and flags bytes 
            int verMajor = _bitStream.ReadRawInt(8);
            int verMinor = _bitStream.ReadRawInt(8);
            int flags = _bitStream.ReadRawInt(8);

            // get the size (in bytes) to skip
            int skip = 0;
            for (int i = 0; i < 4; i++)
            {
                int x = _bitStream.ReadRawUInt(8);
                skip <<= 7;
                skip |= (x & 0x7f);
            }

            // skip the rest of the tag
            _bitStream.ReadByteBlockAlignedNoCRC(null, skip);
        }

        private void FindFrameSync()
        {
            bool first = true;
            //int cnt=0;

            // If we know the total number of samples in the stream, stop if we've read that many.
            // This will stop us, for example, from wasting time trying to sync on an ID3V1 tag.
            if (_streamInfo != null && (_streamInfo.TotalSamples != 0))
            {
                if (_samplesDecoded >= _streamInfo.TotalSamples)
                {
                    //state = DECODER_END_OF_STREAM;
                    return;
                }
            }

            // make sure we're byte aligned
            if (!_bitStream.IsConsumedByteAligned())
            {
                _bitStream.ReadRawUInt(_bitStream.BitsLeftForByteAlignment());
            }

            int x;
            try
            {
                while (_run)
                {
                    x = _bitStream.ReadRawUInt(8);
                    if (x == 0xff)
                    {
                        // MAGIC NUMBER for the first 8 frame sync bits
                        _headerWarmup[0] = (byte) x;
                        x = _bitStream.PeekRawUInt(8);

                        /* we have to check if we just read two 0xff's in a row; the second may actually be the beginning of the sync code */
                        /* else we have to check if the second byte is the end of a sync code */
                        if (x >> 2 == 0x3e)
                        {
                            /* MAGIC NUMBER for the last 6 sync bits */
                            _headerWarmup[1] = (byte) _bitStream.ReadRawUInt(8);
                            //state = DECODER_READ_FRAME;
                            return;
                        }
                    }
                    if (first)
                    {
                        _frameListeners.ProcessError(string.Format("FindSync LOST_SYNC: {0}", x & 0xff));
                        first = false;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                if (!first) _frameListeners.ProcessError("FindSync LOST_SYNC: Left over data in file");
                //state = DECODER_END_OF_STREAM;
            }
        }

        /**
         * Read the next data frame.
         * @throws IOException  On read error
         * @throws FrameDecodeException On frame decoding error
         */
        private bool _firstFrame = true;
        private long _firstFrameOffset;
        public void ReadFrame()
        {
            if (_firstFrame)
            {
                _firstFrame = false;
                _firstFrameOffset = _inputStream.Position;
            }
            int channel;
            //int x;

            /* init the CRC */
            int frameCrc = 0;
            frameCrc = Crc16.Update(_headerWarmup[0], frameCrc);
            frameCrc = Crc16.Update(_headerWarmup[1], frameCrc);
            _bitStream.ResetReadCRC16(frameCrc);

            try
            {
                _frame.Header = new Header(_bitStream, _headerWarmup, _streamInfo);
            }
            catch (BadHeaderException e)
            {
                _frameListeners.ProcessError("Found bad header: " + e);
                throw new FrameDecodeException("Bad Frame Header: " + e);
            }
            //if (state == DECODER_SEARCH_FOR_FRAME_SYNC) return false;
            AllocateOutput(_frame.Header.BlockSize, _frame.Header.Channels);
            for (channel = 0; channel < _frame.Header.Channels; channel++)
            {
                // first figure the correct bits-per-sample of the subframe
                int bps = _frame.Header.BitsPerSample;
                switch (_frame.Header.ChannelAssignment)
                {
                    case Constants.ChannelAssignmentIndependent:
                        /* no adjustment needed */
                        break;
                    case Constants.ChannelAssignmentLeftSide:
                        if (channel == 1)
                            bps++;
                        break;
                    case Constants.ChannelAssignmentRightSide:
                        if (channel == 0)
                            bps++;
                        break;
                    case Constants.ChannelAssignmentMidSide:
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
                    _frameListeners.ProcessError("ReadSubframe: " + e);
                    throw;
                }
            }
            ReadZeroPadding();

            // Read the frame CRC-16 from the footer and check
            frameCrc = _bitStream.GetReadCRC16();
            _frame.CRC = _bitStream.ReadRawUInt(FrameFooterCrcLen);
            if ((frameCrc & 0xffff) == _frame.CRC)
            {
                /* Undo any special channel coding */
                int i;
                switch (_frame.Header.ChannelAssignment)
                {
                    case Constants.ChannelAssignmentIndependent:
                        /* do nothing */
                        break;
                    case Constants.ChannelAssignmentLeftSide:
                        for (i = 0; i < _frame.Header.BlockSize; i++)
                            _channelData[1].Output[i] = _channelData[0].Output[i] - _channelData[1].Output[i];
                        break;
                    case Constants.ChannelAssignmentRightSide:
                        for (i = 0; i < _frame.Header.BlockSize; i++)
                            _channelData[0].Output[i] += _channelData[1].Output[i];
                        break;
                    case Constants.ChannelAssignmentMidSide:
                        for (i = 0; i < _frame.Header.BlockSize; i++)
                        {
                            int mid = _channelData[0].Output[i];
                            int side = _channelData[1].Output[i];
                            mid <<= 1;
                            if ((side & 1) != 0) // i.e. if 'side' is odd...
                                mid++;
                            int left = mid + side;
                            int right = mid - side;
                            _channelData[0].Output[i] = left >> 1;
                            _channelData[1].Output[i] = right >> 1;
                        }
                        //System.exit(1);
                        break;
                }
            }
            else
            {
                // Bad frame, emit error and zero the output signal
                _frameListeners.ProcessError(string.Format("CRC Error: {0} vs {1}", ((frameCrc & 0xffff)),
                                                          ((_frame.CRC & 0xffff))));
                for (channel = 0; channel < _frame.Header.Channels; channel++)
                {
                    for (int j = 0; j < _frame.Header.BlockSize; j++)
                        _channelData[channel].Output[j] = 0;
                }
            }

            // put the latest values into the public section of the decoder instance
            _channels = _frame.Header.Channels;
            _channelAssignment = _frame.Header.ChannelAssignment;
            _bitsPerSample = _frame.Header.BitsPerSample;
            _sampleRate = _frame.Header.SampleRate;
            _blockSize = _frame.Header.BlockSize;

            //samplesDecoded = frame.header.sampleNumber + frame.header.blockSize;
            _samplesDecoded += _frame.Header.BlockSize;
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
                _frameListeners.ProcessFrame(ref _frame);
                return _frame;
            }
            catch (FrameDecodeException)
            {
                _badFrames++;
                return returnnull? null:new Frame();
            }
        }

        private void ReadSubframe(int channel, int bps)
        {
            int x;

            x = _bitStream.ReadRawUInt(8); /* MAGIC NUMBER */

            bool haveWastedBits = ((x & 1) != 0);
            x &= 0xfe;

            int wastedBits = 0;
            if (haveWastedBits)
            {
                wastedBits = _bitStream.ReadUnaryUnsigned() + 1;
                bps -= wastedBits;
            }

            // Lots of magic numbers here
            if ((x & 0x80) != 0)
            {
                _frameListeners.ProcessError(string.Format("ReadSubframe LOST_SYNC: {0}", (x & 0xff)));
                //state = DECODER_SEARCH_FOR_FRAME_SYNC;
                throw new FrameDecodeException(string.Format("ReadSubframe LOST_SYNC: {0}", (x & 0xff)));
                //return true;
            }
            if (x == 0)
            {
                _frame.SubFrames[channel] = new ChannelConstant(_bitStream, _frame.Header, ref _channelData[channel], bps,
                    wastedBits);
            }
            else if (x == 2)
            {
                _frame.SubFrames[channel] = new ChannelVerbatim(_bitStream, _frame.Header, ref _channelData[channel], bps,
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
                _frame.SubFrames[channel] = new ChannelFixed(_bitStream, _frame.Header, ref _channelData[channel], bps,
                    wastedBits, (x >> 1) & 7);
            }
            else if (x < 64)
            {
                //state = DECODER_UNPARSEABLE_STREAM;
                throw new FrameDecodeException(string.Format("ReadSubframe Bad Subframe Type: {0}", (x & 0xff)));
            }
            else
            {
                _frame.SubFrames[channel] = new ChannelLPC(_bitStream, _frame.Header, ref _channelData[channel], bps,
                    wastedBits,
                    ((x >> 1) & 31) + 1);
            }
            if (haveWastedBits)
            {
                int i;
                x = _frame.SubFrames[channel].WastedBits;
                for (i = 0; i < _frame.Header.BlockSize; i++)
                    _channelData[channel].Output[i] <<= x;
            }
        }

        private void ReadZeroPadding()
        {
            if (!_bitStream.IsConsumedByteAligned())
            {
                int zero = _bitStream.ReadRawUInt(_bitStream.BitsLeftForByteAlignment());
                if (zero != 0)
                {
                    _frameListeners.ProcessError(string.Format("ZeroPaddingError: {0}", (zero)));
                    //state = DECODER_SEARCH_FOR_FRAME_SYNC;
                    throw new FrameDecodeException(string.Format("ZeroPaddingError: {0}", (zero)));
                }
            }
        }

        public long GetSamplesDecoded()
        {
            return _samplesDecoded;
        }

        public int GetBadFrames()
        {
            return _badFrames;
        }

        public bool IsEof()
        {
            return _eof;
        }

        public Frame GetLastFrame()
        {
            return _frame;
        }

        // Ported from libFlac
        public bool Seek(long target)
        {
            int approxBytesPerFrame;
	        bool firstSeek = true;
            var seekTable = _metadataList.Find(meta => meta is SeekTable) as SeekTable;

	        /* use values from stream info if we didn't decode a frame */
		    int channels = _channels < 1 ? _streamInfo.Channels : _channels;
		    int bps = _bitsPerSample < 1 ? _streamInfo.BitsPerSample : _bitsPerSample;

	        /* we are just guessing here */
	        if(_streamInfo.MaxFrameSize > 0)
		        approxBytesPerFrame = (_streamInfo.MaxFrameSize + _streamInfo.MinFrameSize) / 2 + 1;
	        /*
	         * Check if it's a known fixed-blocksize stream.  Note that though
	         * the spec doesn't allow zeroes in the STREAMINFO block, we may
	         * never get a STREAMINFO block when decoding so the value of
	         * min_blocksize might be zero.
	         */
	        else if(_streamInfo.MinBlockSize == _streamInfo.MaxBlockSize && _streamInfo.MinBlockSize > 0)
		        approxBytesPerFrame = _streamInfo.MinBlockSize * channels * bps/8 + 64;
	        else
		        approxBytesPerFrame = 4096 * channels * bps/8 + 64;

	        /*
	         * First, we set an upper and lower bound on where in the
	         * stream we will search.  For now we assume the worst case
	         * scenario, which is our best guess at the beginning of
	         * the first frame and end of the stream.
	         */
	        long lowerBound = _firstFrameOffset;
	        long lowerBoundSample = 0;
	        long upperBound = _inputStream.Length;
	        long upperBoundSample = _streamInfo.TotalSamples > 0 ? _streamInfo.TotalSamples : target;

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
                                                                      (p.SampleNumber < _streamInfo.TotalSamples) &&
                                                                      p.SampleNumber <= target);
		        if(lowerPoint != default(SeekPoint)) 
                { /* i.e. we found a suitable seek point... */
			        newLowerBound = _firstFrameOffset + lowerPoint.StreamOffset;
			        newLowerBoundSample = lowerPoint.SampleNumber;
		        }

                /* find the closest seek point > target_sample, if it exists */
                var upperPoint = seekTable.Points.FirstOrDefault(p => p.SampleNumber != long.MinValue &&
                                                                      p.FrameSamples > 0 &&
                                                                      (p.SampleNumber < _streamInfo.TotalSamples) &&
                                                                      p.SampleNumber > target);
		        if(upperPoint != default(SeekPoint)) 
                { /* i.e. we found a suitable seek point... */
			        newUpperBound = _firstFrameOffset + upperPoint.StreamOffset;
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
            _inputStream.Position = target == 0 ? _firstFrameOffset : lowerBound;
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
                if (target >= _frame.Header.SampleNumber - _frame.Header.BlockSize && target <= _frame.Header.SampleNumber)
                    break;

		        long thisFrameSample = _frame.Header.SampleNumber;
                if (thisFrameSample == -1)
                    return false; //throw

		        if (0 == _samplesDecoded || (thisFrameSample + _frame.Header.BlockSize >= upperBoundSample && !firstSeek))
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
			        upperBoundSample = thisFrameSample + _frame.Header.BlockSize;
                    upperBound -= _bitStream.GETInputBytesUnconsumed();
			        approxBytesPerFrame = (int)(2 * (upperBound - pos) / 3 + 16);
		        }
		        else
                { /* target_sample >= this_frame_sample + this frame's blocksize */
			        lowerBoundSample = thisFrameSample + _frame.Header.BlockSize;
                    lowerBound -= _bitStream.GETInputBytesUnconsumed();
			        approxBytesPerFrame = (int)(2 * (lowerBound - pos) / 3 + 16);
		        }
	        }
	        return true;
        }

        public void Dispose()
        {
            _inputStream.Close();
            _inputStream.Dispose();
        }
    }
}