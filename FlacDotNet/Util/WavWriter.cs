using System;
using System.IO;
using FlacDotNet.Frames;
using FlacDotNet.Meta;

namespace FlacDotNet.Util
{
    public class WavWriter
    {
        private const int MAX_BLOCK_SIZE = 65535;
        private readonly DataOutput os;
        private readonly LittleEndianDataOutput osLE;

        private readonly byte[] s8buffer = new byte[MAX_BLOCK_SIZE*Constants.MAX_CHANNELS*4];
                                /* WATCHOUT: can be up to 2 megs */

        private int bps;
        private int channels;
        private long dataOffset;

        private int frameCounter;

        //private bool needsFixup = false;
        private long riffOffset;
        private int sampleRate;
        private int samplesProcessed;
        private long totalSamples;

        public WavWriter(Stream os, StreamInfo streamInfo)
        {
            this.os = new DataOutput(os);
            osLE = new LittleEndianDataOutput(os);
            totalSamples = streamInfo.TotalSamples;
            channels = streamInfo.Channels;
            bps = streamInfo.BitsPerSample;
            sampleRate = streamInfo.SampleRate;
        }

        public WavWriter(Stream os)
        {
            this.os = new DataOutput(os);
            osLE = new LittleEndianDataOutput(os);
        }

        public void WriteHeader()
        {
            long dataSize = totalSamples*channels*((bps + 7)/8);
            if (totalSamples == 0)
            {
                //if (!(os instanceof RandomAccessFile)) throw new IOException("Cannot seek in output stream");
                //needsFixup = true;
                throw new FormatException("Do not support RandomAccessFile");
            }
            //if (dataSize >= 0xFFFFFFDC) throw new IOException("ERROR: stream is too big to fit in a single file chunk (Datasize="+dataSize+")");

            os.Write("RIFF");
            //if (needsFixup) riffOffset = ((System.IO.RandomAccessFile) os).getFilePointer();
            osLE.Write((int) dataSize + 36); // filesize-8
            os.Write("WAVEfmt ");
            os.Write(new byte[] {0x10, 0x00, 0x00, 0x00}); // chunk size = 16
            os.Write(new byte[] {0x01, 0x00}); // compression code == 1
            osLE.WriteShort(channels);
            osLE.Write(sampleRate);
            osLE.Write(sampleRate*channels*((bps + 7)/8)); // or is it (sample_rate*channels*bps) / 8
            osLE.WriteShort(channels*((bps + 7)/8)); // block align
            osLE.WriteShort(bps); // bits per sample
            os.Write("data");
            //if (needsFixup) dataOffset = ((RandomAccessFile) os).getFilePointer();

            osLE.Write((int) dataSize); // data size

            //if (UpateBufferAction != null)
            //{
            //    var bytes = new byte[os.BaseStream.Length];
            //    os.BaseStream.Read(bytes, 0, (int)os.BaseStream.Length);
            //    UpateBufferAction.Invoke(bytes);
            //}
        }

        public void WriteHeader(StreamInfo streamInfo)
        {
            totalSamples = streamInfo.TotalSamples;
            channels = streamInfo.Channels;
            bps = streamInfo.BitsPerSample;
            sampleRate = streamInfo.SampleRate;
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
            var doutLE = new LittleEndianDataOutput(memorystream);
            long dataSize = totalSamples * channels * ((bps + 7) / 8);
            //if (dataSize >= 0xFFFFFFDC) throw new IOException("ERROR: stream is too big to fit in a single file chunk (Datasize="+dataSize+")");

            //dout.Write("RIFF");
            //if (needsFixup) riffOffset = ((System.IO.RandomAccessFile) os).getFilePointer();
            //doutLE.Write((int)dataSize + 36); // filesize-8
            //dout.Write("WAVEfmt ");
            //dout.Write(new byte[] { 0x10, 0x00, 0x00, 0x00 }); // chunk size = 16
            dout.Write(new byte[] { 0x01, 0x00 }); // compression code == 1
            doutLE.WriteShort(channels);
            doutLE.Write(sampleRate);
            doutLE.Write(sampleRate * channels * ((bps + 7) / 8)); // or is it (sample_rate*channels*bps) / 8
            doutLE.WriteShort(channels * ((bps + 7) / 8)); // block align
            doutLE.WriteShort(bps); // bits per sample
            //dout.Write("data");
            //if (needsFixup) dataOffset = ((RandomAccessFile) os).getFilePointer();

            doutLE.Write((int)dataSize); // data size
            dout.Flush();
            doutLE.Flush();
            var buffer = new byte[memorystream.Length];
            memorystream.Seek(0, SeekOrigin.Begin);
            memorystream.Read(buffer, 0, (int) memorystream.Length);
            return buffer;
        }

        public void WriteFrame(Frame frame, ChannelData[] channelData)
        {
            bool isUnsignedSamples = bps <= 8;
            int wideSamples = frame.Header.BlockSize;
            int wideSample;
            int sample;
            int channel;

            if (wideSamples > 0)
            {
                samplesProcessed += wideSamples;
                frameCounter++;
                if (bps == 8)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                            {
                                //System.out.print("("+(int)((byte)(channelData[channel].getOutput()[wideSample] + 0x80))+")");
                                s8buffer[sample++] = (byte) (channelData[channel].Output[wideSample] + 0x80);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                                s8buffer[sample++] = (byte) (channelData[channel].Output[wideSample]);
                    }
                    os.Write(s8buffer, 0, sample);
                }
                else if (bps == 16)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                            {
                                var val = (short) (channelData[channel].Output[wideSample] + 0x8000);
                                s8buffer[sample++] = (byte) (val & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 8) & 0xff);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                            {
                                var val = (short) (channelData[channel].Output[wideSample]);
                                s8buffer[sample++] = (byte) (val & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 8) & 0xff);
                            }
                    }
                    os.Write(s8buffer, 0, sample);
                }
                else if (bps == 24)
                {
                    if (isUnsignedSamples)
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                            {
                                int val = (channelData[channel].Output[wideSample] + 0x800000);
                                s8buffer[sample++] = (byte) (val & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 8) & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 16) & 0xff);
                            }
                    }
                    else
                    {
                        for (sample = wideSample = 0; wideSample < wideSamples; wideSample++)
                            for (channel = 0; channel < channels; channel++)
                            {
                                int val = (channelData[channel].Output[wideSample]);
                                s8buffer[sample++] = (byte) (val & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 8) & 0xff);
                                s8buffer[sample++] = (byte) ((val >> 16) & 0xff);
                            }
                    }
                    os.Write(s8buffer, 0, sample);
                }
            }
        }

        public void WritePCM(ByteData space)
        {
            os.Write(space.Data, 0, space.Length);
        }

    }
}