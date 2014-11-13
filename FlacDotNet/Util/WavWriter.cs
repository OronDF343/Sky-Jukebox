using System;
using System.IO;
using FlacDotNet.Frames;
using FlacDotNet.Meta;

namespace FlacDotNet.Util
{
    public class WavWriter
    {
        private const int MaxBlockSize = 65535;
        private readonly DataOutput _os;
        private readonly LittleEndianDataOutput _osLe;

        private readonly byte[] _s8Buffer = new byte[MaxBlockSize*Constants.MaxChannels*4];
                                /* WATCHOUT: can be up to 2 megs */

        private int _bps;
        private int _channels;
        private long _dataOffset;

        private int _frameCounter;

        //private bool needsFixup = false;
        private long _riffOffset;
        private int _sampleRate;
        private int _samplesProcessed;
        private long _totalSamples;

        public WavWriter(Stream os, StreamInfo streamInfo)
        {
            _os = new DataOutput(os);
            _osLe = new LittleEndianDataOutput(os);
            _totalSamples = streamInfo.TotalSamples;
            _channels = streamInfo.Channels;
            _bps = streamInfo.BitsPerSample;
            _sampleRate = streamInfo.SampleRate;
        }

        public WavWriter(Stream os)
        {
            _os = new DataOutput(os);
            _osLe = new LittleEndianDataOutput(os);
        }

        public void WriteHeader()
        {
            long dataSize = _totalSamples*_channels*((_bps + 7)/8);
            if (_totalSamples == 0)
            {
                //if (!(os instanceof RandomAccessFile)) throw new IOException("Cannot seek in output stream");
                //needsFixup = true;
                throw new FormatException("Do not support RandomAccessFile");
            }
            //if (dataSize >= 0xFFFFFFDC) throw new IOException("ERROR: stream is too big to fit in a single file chunk (Datasize="+dataSize+")");

            _os.Write("RIFF");
            //if (needsFixup) riffOffset = ((System.IO.RandomAccessFile) os).getFilePointer();
            _osLe.Write((int) dataSize + 36); // filesize-8
            _os.Write("WAVEfmt ");
            _os.Write(new byte[] {0x10, 0x00, 0x00, 0x00}); // chunk size = 16
            _os.Write(new byte[] {0x01, 0x00}); // compression code == 1
            _osLe.WriteShort(_channels);
            _osLe.Write(_sampleRate);
            _osLe.Write(_sampleRate*_channels*((_bps + 7)/8)); // or is it (sample_rate*channels*bps) / 8
            _osLe.WriteShort(_channels*((_bps + 7)/8)); // block align
            _osLe.WriteShort(_bps); // bits per sample
            _os.Write("data");
            //if (needsFixup) dataOffset = ((RandomAccessFile) os).getFilePointer();

            _osLe.Write((int) dataSize); // data size

            //if (UpateBufferAction != null)
            //{
            //    var bytes = new byte[os.BaseStream.Length];
            //    os.BaseStream.Read(bytes, 0, (int)os.BaseStream.Length);
            //    UpateBufferAction.Invoke(bytes);
            //}
        }

        public void WriteHeader(StreamInfo streamInfo)
        {
            _totalSamples = streamInfo.TotalSamples;
            _channels = streamInfo.Channels;
            _bps = streamInfo.BitsPerSample;
            _sampleRate = streamInfo.SampleRate;
            WriteHeader();
        }

        public static byte[]GetHeaderBytes(StreamInfo streamInfo)
        {
            var totalSamples = streamInfo.TotalSamples;
            var channels = streamInfo.Channels;
            var bps = streamInfo.BitsPerSample;
            var sampleRate = streamInfo.SampleRate;
            var memorystream = new MemoryStream();
            var dout = new DataOutput(memorystream);
            var doutLe = new LittleEndianDataOutput(memorystream);
            long dataSize = totalSamples * channels * ((bps + 7) / 8);
            //if (dataSize >= 0xFFFFFFDC) throw new IOException("ERROR: stream is too big to fit in a single file chunk (Datasize="+dataSize+")");

            //dout.Write("RIFF");
            //if (needsFixup) riffOffset = ((System.IO.RandomAccessFile) os).getFilePointer();
            //doutLE.Write((int)dataSize + 36); // filesize-8
            //dout.Write("WAVEfmt ");
            //dout.Write(new byte[] { 0x10, 0x00, 0x00, 0x00 }); // chunk size = 16
            dout.Write(new byte[] { 0x01, 0x00 }); // compression code == 1
            doutLe.WriteShort(channels);
            doutLe.Write(sampleRate);
            doutLe.Write(sampleRate * channels * ((bps + 7) / 8)); // or is it (sample_rate*channels*bps) / 8
            doutLe.WriteShort(channels * ((bps + 7) / 8)); // block align
            doutLe.WriteShort(bps); // bits per sample
            //dout.Write("data");
            //if (needsFixup) dataOffset = ((RandomAccessFile) os).getFilePointer();

            doutLe.Write((int)dataSize); // data size
            dout.Flush();
            doutLe.Flush();
            var buffer = new byte[memorystream.Length];
            memorystream.Seek(0, SeekOrigin.Begin);
            memorystream.Read(buffer, 0, (int) memorystream.Length);
            return buffer;
        }

        public void WriteFrame(Frame frame, ChannelData[] channelData)
        {
            bool isUnsignedSamples = _bps <= 8;
            int wideSamples = frame.Header.BlockSize;

            if (wideSamples > 0)
            {
                _samplesProcessed += wideSamples;
                _frameCounter++;
                int wideSample;
                int sample;
                int channel;
                if (_bps == 8)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                            {
                                //System.out.print("("+(int)((byte)(channelData[channel].getOutput()[wideSample] + 0x80))+")");
                                _s8Buffer[sample++] = (byte) (channelData[channel].Output[wideSample] + 0x80);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                                _s8Buffer[sample++] = (byte) (channelData[channel].Output[wideSample]);
                    }
                    _os.Write(_s8Buffer, 0, sample);
                }
                else if (_bps == 16)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                            {
                                var val = (short) (channelData[channel].Output[wideSample] + 0x8000);
                                _s8Buffer[sample++] = (byte) (val & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 8) & 0xff);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                            {
                                var val = (short) (channelData[channel].Output[wideSample]);
                                _s8Buffer[sample++] = (byte) (val & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 8) & 0xff);
                            }
                    }
                    _os.Write(_s8Buffer, 0, sample);
                }
                else if (_bps == 24)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                            {
                                int val = (channelData[channel].Output[wideSample] + 0x800000);
                                _s8Buffer[sample++] = (byte) (val & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 8) & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 16) & 0xff);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < _channels; channel++)
                            {
                                int val = (channelData[channel].Output[wideSample]);
                                _s8Buffer[sample++] = (byte) (val & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 8) & 0xff);
                                _s8Buffer[sample++] = (byte) ((val >> 16) & 0xff);
                            }
                    }
                    _os.Write(_s8Buffer, 0, sample);
                }
            }
        }

        public void WritePcm(ByteData space)
        {
            _os.Write(space.Data, 0, space.Length);
        }

    }
}