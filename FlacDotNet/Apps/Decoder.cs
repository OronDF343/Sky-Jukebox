using System;
using System.IO;
using FlacDotNet.Meta;
using FlacDotNet.Util;

namespace FlacDotNet.Apps
{
    public class Decoder : IPCMProcessor
    {
        private WavWriter _wavWriter;
        private FLACDecoder _decoder;
        private Stream _inputStream;

        #region IPCMProcessor Members

        public void ProcessStreamInfo(ref StreamInfo info)
        {
            try
            {
                _wavWriter.WriteHeader(info);
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {

            }
        }

        public void ProcessPCM(ByteData pcm)
        {
            try
            {
                _wavWriter.WritePCM(pcm);
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {

            }
        }

        #endregion

        public bool Decode(String inFileName, String outFileName)
        {
            var ins = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
            if (File.Exists(outFileName))
                File.Delete(outFileName);
            Stream ous = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
            return Decode(ins, ous);
        }

        public bool Decode(Stream inputStream, Stream outputStream)
        {
            _inputStream = inputStream;
            _wavWriter = new WavWriter(outputStream);
            _decoder = new FLACDecoder(inputStream);
            _decoder.AddPCMProcessor(this);
            var result = _decoder.Decode();
            inputStream.Close();
            inputStream.Dispose();
            _inputStream.Close();
            _inputStream.Dispose();
            return result;
        }

        public void StopDecode()
        {
            lock (this)
            {
                if (_decoder != null)
                {
                    _decoder.StopDecode();
                    _decoder.RemovePCMProcessor(this);
                }
                if (_inputStream == null) return;
                _inputStream.Close();
                _inputStream.Dispose();
            }
        }

    }
}