using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FlacBox
{
    /// <summary>
    /// Reads basic FLAC data.
    /// </summary>
    public class FlacReader : IDisposable
    {
        bool leaveOpen;
        Stream baseStream;

        public Stream BaseStream
        {
            get { return baseStream; }
        }

        FlacRecordType recordType = FlacRecordType.None;

        public FlacRecordType RecordType
        {
            get { return recordType; }
        }

        public int FrameNumber
        {
            get { return frameNumber; }
        }

        public int CurrentChannel
        {
            get { return subframeIndex; }
        }

        bool isLastMetadataBlock = false;
        FlacBitStreamReader bitReader = null;
        int wastedBitsPerSample = 0;

        FlacMetadataBlockType metadataBlockType = FlacMetadataBlockType.Invalid;

        public FlacMetadataBlockType MetadataBlockType
        {
            get { return metadataBlockType; }
        }

        FlacStreaminfo streaminfo;

        public FlacStreaminfo Streaminfo
        {
            get { return streaminfo; }
        }

        int sampleSizeInBits;

        public int FrameBitsPerSample
        {
            get { return sampleSizeInBits; }
        }

        int sampleRate;

        public int FrameSampleRate
        {
            get { return sampleRate; }
        }

        int channelCount;

        public int FrameChannelCount
        {
            get { return channelCount; }
        }

        int blockSize;

        public int BlockSize
        {
            get { return blockSize; }
        }

        SoundChannelAssignment[] channelAssignment = null;

        public SoundChannelAssignment[] ChannelAssignment
        {
            get { return channelAssignment; }
        }

        SoundChannelAssignmentType channelAssignmentType = SoundChannelAssignmentType.None;

        public SoundChannelAssignmentType ChannelAssignmentType
        {
            get { return channelAssignmentType; }
        }

        public FlacReader(Stream baseStream, bool leaveOpen)
        {
            this.baseStream = baseStream;
            this.leaveOpen = leaveOpen;
        }

        public bool Read()
        {
            if (recordType == FlacRecordType.Eof)
                return false;

            try
            {
                switch (recordType)
                {
                    case FlacRecordType.None:
                        ReadStream();
                        break;
                    case FlacRecordType.Stream:
                        ReadMetadataBlock();
                        break;
                    case FlacRecordType.MetadataBlock:
                        if (isLastMetadataBlock)
                            ReadFrame();
                        else
                            ReadMetadataBlock();
                        break;
                    case FlacRecordType.Frame:
                        subframeIndex = 0;
                        ReadSubframe();
                        break;
                    case FlacRecordType.Subframe:
                        if (!dataRead)
                        {
                            SkipSampleValues();
                        }

                        if (++subframeIndex < FrameChannelCount)
                            ReadSubframe();
                        else
                            ReadFrameFooter();
                        break;
                    case FlacRecordType.FrameFooter:
                        ReadFrame();
                        break;
                    case FlacRecordType.Error:
                        throw new FlacException("Reader in error state");
                    case FlacRecordType.Sync:
                        ReadFrame();
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return recordType != FlacRecordType.Eof;
            }
            catch
            {
                recordType = FlacRecordType.Error;
                throw;
            }
        }

        byte secondSyncByte = 2; // set to invalid reserved value

        public bool FindSync()
        {
            const byte FirstSyncByte = 0xFF;
            const byte SecondSyncByte = 0xF8;
            const byte SecondSyncByteMask = 0xFC;

            var b = BaseStream.ReadByte();
            var found = false;
            while (b >= 0)
            {
                if (b == FirstSyncByte)
                {
                    b = BaseStream.ReadByte();
                    if (b >= 0 && (b & SecondSyncByteMask) == SecondSyncByte)
                    {
                        // sync found
                        secondSyncByte = (byte)b;
                        recordType = FlacRecordType.Sync;
                        found = true;
                        break;
                    }
                }
                else
                {
                    b = BaseStream.ReadByte();
                }
            }
            return found;
        }

        private void SkipSampleValues()
        {
            if(!dataRead)
            {
                foreach (var value in dataSource) ;                

                dataRead = true;
            }
        }

        public IEnumerable<int> GetValues()
        {
            if (RecordType == FlacRecordType.Subframe)
                return ReadSubframeValues();
            else if (RecordType == FlacRecordType.Frame)
            {
                var mixer = WaveSampleMixerFactory.CreateWaveSampleMixer(ChannelAssignment);
                return mixer.MixSamples(this);
            }
            else
                throw new FlacException("Reader shall be pointing to frame or subframe");
        }

        public int[] ReadSubframeValues()
        {
            if (dataRead) 
                throw new FlacException("Cannot read twice");

            var values = new int[BlockSize];
            var i = 0;
            foreach(var value in dataSource)
            {
                values[i++] = value << wastedBitsPerSample;
            }

            Debug.Assert(i == values.Length);

            dataRead = true;

            return values;
        }

        IEnumerable<int> dataSource = null;

        private void ReadFrameFooter()
        {
            var crc16 = bitReader.Complete();
            bitReader = null;

            var data = ReadExactly(2);
            var footerCrc16 = data[0] << 8 | data[1];

            if (crc16 != footerCrc16)
                throw new FlacException("Invalid frame footer CRC16");

            subframeIndex = 0;

            recordType = FlacRecordType.FrameFooter;
        }

        private void ReadSubframe()
        {
            var zeroPadding = bitReader.ReadBits(1);
            if (zeroPadding != 0)
                throw new FlacException("Subframe zero padding is not zero");
            var subframeType = (int)bitReader.ReadBits(6);

            SubframeType type;
            var order = 0;
            if (subframeType == FlacCommons.ConstantSubframeType)
                type = SubframeType.SubframeConstant;
            else if (subframeType == FlacCommons.VerbatimSubframeType)
                type = SubframeType.SubframeVerbatim;
            else if (FlacCommons.FixedSubframeTypeStart <= subframeType &&
                subframeType <= FlacCommons.FixedSubframeTypeEnd)
            {
                type = SubframeType.SubframeFixed;
                order = subframeType - FlacCommons.FixedSubframeTypeStart;
            }
            else if (subframeType >= FlacCommons.LpcSubframeTypeStart)
            {
                type = SubframeType.SubframeLpc;
                order = subframeType - FlacCommons.LpcSubframeTypeStart + 1;
            }
            else
                throw new FlacException("Subframe type is set to reserved");

            var wastedBitsPerSampleFlag = bitReader.ReadBits(1);
            if (wastedBitsPerSampleFlag > 0)
            {
                wastedBitsPerSample = 1 + (int)bitReader.ReadUnary();
            }
            else
                wastedBitsPerSample = 0;

            this.subframeType = type;

            var subframeBitsPerSample = FrameBitsPerSample;
            if (ChannelAssignment[subframeIndex] == SoundChannelAssignment.Difference)
            {
                subframeBitsPerSample++; // undocumented
            }

            switch (type)
            {
                case SubframeType.SubframeConstant:
                    PrepareConstantSubframe(subframeBitsPerSample);
                    break;
                case SubframeType.SubframeVerbatim:
                    PrepareVerbatimSubframe(subframeBitsPerSample);
                    break;
                case SubframeType.SubframeLpc:
                    PrepareLpcSubframe(order, subframeBitsPerSample);
                    break;
                case SubframeType.SubframeFixed:
                    PrepareFixedSubframe(order, subframeBitsPerSample);
                    break;
            }

            recordType = FlacRecordType.Subframe;
            dataRead = false;
        }

        private void PrepareConstantSubframe(int subframeBitsPerSample)
        {
            var sample = bitReader.ReadSignedBits(subframeBitsPerSample);
            dataSource = GetNValues(sample, BlockSize);
        }

        private IEnumerable<int> GetNValues(int value, int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return value;
            }
        }

        private void PrepareVerbatimSubframe(int subframeBitsPerSample)
        {
            dataSource = ReadValuesVerbatim(bitReader, BlockSize, subframeBitsPerSample);
        }

        private IEnumerable<int> ReadValuesVerbatim(FlacBitStreamReader bitReader, int blockSize, int bitsPerSample)
        {
            for (var i = 0; i < blockSize; i++)
            {
                var value = bitReader.ReadSignedBits(bitsPerSample);
                yield return value;
            }
        }

        private void PrepareFixedSubframe(int order, int subframeBitsPerSample)
        {
            var warmupSamples = new int[order];
            for (var i = 0; i < order; i++)
            {
                warmupSamples[i] = bitReader.ReadSignedBits(subframeBitsPerSample - wastedBitsPerSample);
            }

            var residual = ReadResidualData(bitReader, BlockSize, order);
            var predictor = PredictorFactory.CreateFixedPredictor(order, RemoveLastItem(warmupSamples));
            dataSource = GetPredictorSamples(BlockSize, warmupSamples, predictor, residual);
        }

        private void PrepareLpcSubframe(int order, int subframeBitsPerSample)
        {
            const int InvalidLpcPrecision = 15;
            
            var warmupSamples = new int[order];
            for (var i = 0; i < order; i++)
            {
                warmupSamples[i] = bitReader.ReadSignedBits(subframeBitsPerSample - wastedBitsPerSample);
            }
            var precisionCode = bitReader.ReadBits(4);
            if (precisionCode == InvalidLpcPrecision)
                throw new FlacException("Invalid subframe coefficient precision");

            var precision = (int)(precisionCode + 1);

            var shift = bitReader.ReadSignedBits(5);

            var coefficients = new int[order];
            for (var i = 0; i < order; i++)
            {
                coefficients[i] = bitReader.ReadSignedBits(precision);
            }

            var residual = ReadResidualData(bitReader, BlockSize, order);
            var predictor = PredictorFactory.CreateLpcPredictor(coefficients, shift, RemoveLastItem(warmupSamples));
            dataSource = GetPredictorSamples(BlockSize, warmupSamples, predictor, residual);
        }

        private IEnumerator<int> ReadResidualData(FlacBitStreamReader bitReader, int blockSize, int predictorOrder)
        {
            const byte RiceCodingWith4BitParameter = 0;
            const byte RiceCodingWith5BitParameter = 1;

            var residualRiceParamSizeType = bitReader.ReadBits(2);

            int residualRiceParamSize;
            if (residualRiceParamSizeType == RiceCodingWith4BitParameter)
                residualRiceParamSize = 4;
            else if (residualRiceParamSizeType == RiceCodingWith5BitParameter)
                residualRiceParamSize = 5;
            else
                throw new FlacException("Reserved residual coding method");

            // rice and rice2 almost the same
            // read rice partitined method
            var partitionOrder = (int)bitReader.ReadBits(4);
            var partitionCount = 1 << partitionOrder;
            var sampleCount = blockSize >> partitionOrder;

            if (sampleCount < predictorOrder || sampleCount < 1 || 
                (sampleCount << partitionOrder) != blockSize )
                throw new FlacException("Invalid partition order");

            for (var i = 0; i < partitionCount; i++)
            {
                var skipSamples = i == 0 ? predictorOrder : 0;

                var riceParameter = (int)bitReader.ReadBits(residualRiceParamSize);
                if (riceParameter + 1 == 1 << residualRiceParamSize)
                {
                    // escape mode
                    var bitsPerSample = (int)bitReader.ReadBits(5);

                    for (var j = skipSamples; j < sampleCount; j++)
                    {
                        yield return bitReader.ReadSignedBits(bitsPerSample);                        
                    }
                }
                else
                {
                    var maxRiceK = int.MaxValue >> riceParameter;
                    for (var j = skipSamples; j < sampleCount; j++)
                    {
                        yield return bitReader.ReadRice(riceParameter);
                    }
                }
            }
        }

        private static int[] RemoveLastItem(int[] array)
        {
            return ArrayUtils.CutArray(array, 0, array.Length - 1);
        }

        private IEnumerable<int> GetPredictorSamples(int blockSize, int[] warmupSamples, IPredictor predictor, IEnumerator<int> residualData)
        {
            var lastSample = 0;
            if (warmupSamples.Length > 0)
            {
                for (var i = 0; i < warmupSamples.Length; i++)
                {
                    yield return warmupSamples[i];
                }
                lastSample = warmupSamples[warmupSamples.Length - 1];
            }

            for (var i = warmupSamples.Length; i < blockSize; i++)
            {
                if (!residualData.MoveNext())
                    throw new FlacException("Not enough residual data");
                var x = predictor.Next(lastSample);
                var e = residualData.Current;
                var nextSample = x + e;
                yield return nextSample;

                lastSample = nextSample;
            }

            if (residualData.MoveNext())
                throw new FlacException("Not all residual data is decoded");
        }

        private void ReadFrame()
        {
            const int FrameHeaderLength = 2 + 1 + 1;
            const int FrameSync = 0xFFF8;
            const int FrameSyncMask = 0xFFFC;
            const int ReservedBlockSizeSamplesType = 0;
            const int InvalidSampleRateType = 15;

            int read;
            var data = new byte[FrameHeaderLength];
            if (recordType != FlacRecordType.Sync)
            {
                read = BaseStream.Read(data, 0, FrameHeaderLength);
                if (read < FrameHeaderLength)
                {
                    if (read <= 0)
                    {
                        recordType = FlacRecordType.Eof;
                        return;
                    }
                    throw new FlacException("Unexpected eof of stream: invalid frame header length");
                }

                if (((data[0] << 8 | data[1]) & FrameSyncMask) != FrameSync)
                    throw new FlacException("Frame sync is expected");
            }
            else
            {
                const int SyncReadLength = 2;
                read = BaseStream.Read(data, SyncReadLength, FrameHeaderLength - SyncReadLength);
                if(read + SyncReadLength < FrameHeaderLength)
                    throw new FlacException("Unexpected eof of stream: invalid frame header length");

                data[0] = (byte)(FrameSync >> 8); data[1] = secondSyncByte;
            }

            if((data[1] & 0x02) != 0)
                throw new FlacException("Frame header reserved bit (15) shall not be 1");

            variableBlockSize = (data[1] & 0x01) != 0;

            var blockSizeSamplesType = data[2] >> 4;
            if(blockSizeSamplesType == ReservedBlockSizeSamplesType)
                throw new FlacException("Frame header block size samples shall not be set to reserved");

            var previousBlockSize = blockSize;

            blockSize = FlacCommons.StaticBlockSizeSamples[blockSizeSamplesType];

            var sampleRateType = data[2] & 0x0F;
            if(sampleRateType == InvalidSampleRateType)
                throw new FlacException("Frame header sample rate type is invalid");

            sampleRate = FlacCommons.StaticSampleRates[sampleRateType];

            var channelAssignmentType = data[3] >> 4;

            if (channelAssignmentType >= FlacCommons.StaticChannelAssignments.Length)
                throw new FlacException("Frame header channel assignments are defined as reserved");

            this.channelAssignmentType = (SoundChannelAssignmentType)channelAssignmentType;
            channelAssignment = FlacCommons.StaticChannelAssignments[channelAssignmentType];
            if (channelAssignment == null)
                throw new FlacException("Frame header channel assignment are not defined");

            channelCount = channelAssignment.Length;

            var sampleSizeInBitsType = (data[3] >> 1) & 0x07;
            if (sampleSizeInBitsType == FlacCommons.StreaminfoSizeInBitsType)
                sampleSizeInBits = Streaminfo.BitsPerSample;
            else if (FlacCommons.StaticSampleSizeInBits[sampleSizeInBitsType] > 0)
                sampleSizeInBits = FlacCommons.StaticSampleSizeInBits[sampleSizeInBitsType];
            else
                throw new FlacException("Frame header sample size is defined as reserved");

            if((data[3] & 1) != 0)
                throw new FlacException("Frame header reserved bit (31) shall not be 1");

            var ms = new MemoryStream(20);
            ms.Write(data, 0, FrameHeaderLength);

            byte[] numberData;
            if (variableBlockSize)
            {
                ReadUtf8Number(out sampleNumber, out numberData);
                if (numberData.Length > 7)
                    throw new FlacException("Invalid variable block size");
            }
            else
            {
                ReadUtf8Number(out frameNumber, out numberData);
                if (numberData.Length > 6)
                    throw new FlacException("Invalid frame number");
                sampleNumber = frameNumber == 0 ? 0 : 
                    previousBlockSize * frameNumber;                
            }
            ms.Write(numberData, 0, numberData.Length);

            byte[] blockSizeData = null;
            switch (blockSizeSamplesType)
            {
                case FlacCommons.Bit8BlockSizeSamplesType:
                    blockSizeData = ReadExactly(1);
                    blockSize = (int)blockSizeData[0] + 1;
                    break;
                case FlacCommons.Bit16BlockSizeSamplesType:
                    blockSizeData = ReadExactly(2);
                    blockSize = (blockSizeData[0] << 8 | blockSizeData[1]) + 1;
                    break;
            }
            if(blockSizeData != null)
                ms.Write(blockSizeData, 0, blockSizeData.Length);

            byte[] sampleRateData = null;
            switch (sampleRateType)
            {
                case FlacCommons.StreaminfoSampleRateType:
                    sampleRate = Streaminfo.SampleRate;
                    break;
                case FlacCommons.Bit8SampleRateType:
                    sampleRateData = ReadExactly(1);
                    sampleRate = sampleRateData[0];
                    break;
                case FlacCommons.Bit16SampleRateType:
                    sampleRateData = ReadExactly(2);
                    sampleRate = sampleRateData[0] << 8 | sampleRateData[1];
                    break;
                case FlacCommons.Bit16Mult10SampleRateType:
                    sampleRateData = ReadExactly(2);
                    sampleRate = (sampleRateData[0] << 8 | sampleRateData[1]) * 10;
                    break;
            }
            if (sampleRateData != null)
                ms.Write(sampleRateData, 0, sampleRateData.Length);

            var readData = ms.ToArray();
            var crc8 = CrcUtils.Crc8(0, readData);
            var headerCrc8 = BaseStream.ReadByte();
            if (headerCrc8 < 0)
                throw new FlacException("Unexpected end of stream: frame CRC8 expected");
            else if(crc8 != headerCrc8)
                throw new FlacException("Invalid frame CRC");

            var currentCrc16 = CrcUtils.Crc16(
                CrcUtils.Crc16(0, readData), (byte)headerCrc8);

            bitReader = new FlacBitStreamReader(BaseStream, currentCrc16);

            lastFrameHeaderData = new byte[readData.Length + 1];
            Array.Copy(readData, lastFrameHeaderData, readData.Length);
            lastFrameHeaderData[readData.Length] = crc8;

            recordType = FlacRecordType.Frame;
        }

        public byte[] ReadFrameRawData()
        {
            if(recordType != FlacRecordType.Frame)
                throw new FlacException("Cannot read non-frame data");

            var ms = new MemoryStream();
            ms.Write(lastFrameHeaderData, 0, lastFrameHeaderData.Length);

            // replace bitReader
            var currentCrc16 = bitReader.Complete();
            bitReader = new FlacBitStreamReader(
                new SinkedStream(BaseStream, ms), currentCrc16);

            // read subframes
            for (var i = 0; i < FrameChannelCount; i++)
            {
                ReadSubframe();
                SkipSampleValues();
            }
            bitReader.Complete();
            bitReader = null;

            // read footer
            var crc16 = ReadExactly(2);
            recordType = FlacRecordType.FrameFooter;

            var frameDataLength = checked((int)ms.Length);
            ms.Write(crc16, 0, crc16.Length);

            var frameData = ms.ToArray();

            // check CRC16
            var dataCrc16 = CrcUtils.Crc16(0, frameData, 0, frameDataLength);
            var footerCrc = (crc16[0] << 8) | crc16[1];
            if(dataCrc16 != footerCrc)
                throw new FlacException("Invalid frame footer CRC16");

            return frameData;
        }

        byte[] lastFrameHeaderData = null;

        private void ReadUtf8Number(out int number, out byte[] numberData)
        {
            var firstByte = BaseStream.ReadByte();
            if(firstByte < 0)
                throw new FlacException("Unexpected end of stream: UTF8 number expected");
            if (firstByte < 0x80)
            {
                number = firstByte;
                numberData = new byte[] { (byte)firstByte };
            }
            else if (firstByte >= 0xC0) 
            {
                var mask = 0x20;
                var satelitesCount = 1;
                while (mask > 0 && (firstByte & mask) != 0)
                {
                    satelitesCount++;
                    mask >>= 1;
                }
                if(mask == 0)
                    throw new FlacException("Invalid UTF8 number size");
                number = (firstByte & (mask - 1));
                numberData = new byte[satelitesCount + 1];
                numberData[0] = (byte)firstByte;
                for (var i = 0; i < satelitesCount; i++)
                {
                    var nextByte = BaseStream.ReadByte();
                    if (nextByte < 0)
                        throw new FlacException("Unexpected end of stream: UTF8 number satelite expected");
                    if((nextByte & 0xC0) != 0x80)
                        throw new FlacException("Invalid UTF8 number satelite");
                    number = (number << 6) | (nextByte & 0x3F);
                    numberData[i + 1] = (byte)nextByte;
                }
            }
            else
                throw new FlacException("Invalid UTF8 number start");
        }

        bool variableBlockSize;
        int frameNumber;
        int sampleNumber;
        int subframeIndex;
        SubframeType subframeType;
        bool dataRead;

        private void ReadMetadataBlock()
        {
            // read header
            var headerType = BaseStream.ReadByte();
            if (headerType < 0)
                throw new FlacException("Unexepted end of stream: metadata block expected");

            isLastMetadataBlock = (headerType & 0x80) != 0;

            metadataBlockType = (FlacMetadataBlockType)(headerType & 0x7F);

            var metadataBlockLengthBytes = ReadExactly(3);
            var metadataBlockLength = metadataBlockLengthBytes[0] << 16 |
                metadataBlockLengthBytes[1] << 8 |
                metadataBlockLengthBytes[2];

            recordType = FlacRecordType.MetadataBlock;

            if (metadataBlockType == FlacMetadataBlockType.Streaminfo)
            {
                ReadStreaminfo(metadataBlockLength);
            }
            else
            {
                // TODO read other block types
                SkipData(metadataBlockLength);
            }
        }

        private void SkipData(int bytes)
        {
            if (BaseStream.CanSeek)
                BaseStream.Seek(bytes, SeekOrigin.Current);
            else
            {
                const int MaxSkip = 1024;
                var buffer = new byte[MaxSkip];
                while (bytes > MaxSkip)
                {
                    BaseStream.Read(buffer, 0, MaxSkip);
                    bytes -= MaxSkip;
                }
                BaseStream.Read(buffer, 0, bytes);
            }
        }

        private void ReadStreaminfo(int metadataBlockLength)
        {
            const int Md5Length = FlacCommons.Md5Length;
            const int Md5Offset = FlacCommons.StreaminfoMetadataBlockLengh - Md5Length;

            if (metadataBlockLength < FlacCommons.StreaminfoMetadataBlockLengh)
                throw new FlacException("Invalid STREAMINFO block size");

            var data = ReadExactly(metadataBlockLength);

            var streaminfo = new FlacStreaminfo();
            streaminfo.MinBlockSize = data[0] << 8 | data[1];
            streaminfo.MaxBlockSize = data[2] << 8 | data[3];
            streaminfo.MinFrameSize = data[4] << 16 | data[5] << 8 | data[6];
            streaminfo.MaxFrameSize = data[7] << 16 | data[8] << 8 | data[9];

            streaminfo.SampleRate = (data[10] << 16 | data[11] << 8 | data[12]) >> 4;
            streaminfo.ChannelsCount = ((data[12] >> 1) & 0x07) + 1;

            streaminfo.BitsPerSample = ((data[12] & 0x01) << 4 | (data[13] >> 4)) + 1;

            streaminfo.TotalSampleCount = ((long)(data[13] & 0x0F)) << 32 |
                (long)data[14] << 24 | (uint)(data[15] << 16 | data[16] << 8 | data[17]);

            streaminfo.MD5 = new byte[Md5Length];
            Array.Copy(data, Md5Offset, streaminfo.MD5, 0, Md5Length);

            this.streaminfo = streaminfo;
        }

        private byte[] ReadExactly(int bytes)
        {
            var data = new byte[bytes];
            var read = BaseStream.Read(data, 0, data.Length);
            if (read != data.Length)
                throw new FlacException(String.Format("Unexpected end of stream: expected {0} bytes", bytes));
            return data;
        }

        private void ReadStream()
        {
            var marker = new byte[FlacCommons.StreamMarker.Length];
            var read = BaseStream.Read(marker, 0, marker.Length);
            if (read != marker.Length)
                throw new FlacException("Unexpected end of stream: fLaC is expected");

            for (var i = 0; i < marker.Length; i++)
            {
                if (marker[i] != FlacCommons.StreamMarker[i])
                    throw new FlacException("Invalid stream marker: fLaC is expected");
            }

            recordType = FlacRecordType.Stream;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        public void Close()
        {
            Dispose(true);
        }

        bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (!leaveOpen) BaseStream.Close();
                baseStream = null;
                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~FlacReader()
        {
            Dispose(false);
        }

        private class SinkedStream : Stream
        {
            Stream baseStream;
            Stream sinkStream;

            internal SinkedStream(Stream baseStream, Stream sinkStream)
            {
                this.baseStream = baseStream;
                this.sinkStream = sinkStream;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var read = baseStream.Read(buffer, offset, count);
                if (read > 0)
                    sinkStream.Write(buffer, offset, read);
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }

    public enum FlacMetadataBlockType
    {
        Streaminfo, 
        Padding, 
        Application,
        Seektable, 
        VorbisComment, 
        Cuesheet, 
        Picture,
        Invalid = 127
    }

    public enum FlacRecordType
    {
        None,
        Eof,
        Stream, 
        MetadataBlock,
        Frame, 
        FrameFooter, 
        Subframe, 
        Sync,
        Error
    }

}
