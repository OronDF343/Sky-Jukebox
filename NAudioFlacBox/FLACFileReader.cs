using FlacBox;
using NAudio.Wave;
using System.IO;
using System.Threading;

namespace NAudioFlacBox
{
    public class FlacFileReader : WaveStream
    {
        private readonly WaveOverFlacStream _flacStream;
        private readonly FlacReader _flacReader;
        private readonly Stream _fileStream;
        private readonly FlacStreaminfo _streamInfo;

        public FlacFileReader(string fileName)
        {
            _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _flacReader = new FlacReader(_fileStream, true);
            while (_flacReader.Streaminfo == null)
                _flacReader.Read();
            _streamInfo = _flacReader.Streaminfo;
            _flacReader.Close();
            _fileStream.Position = 0;
            _flacReader = new FlacReader(_fileStream, false);
            _flacStream = new WaveOverFlacStream(_flacReader);
        }
        public override WaveFormat WaveFormat
        {
            get { return new WaveFormat(_streamInfo.SampleRate, _streamInfo.BitsPerSample, _streamInfo.ChannelsCount); }
        }

        public override long Length
        {
            get { return _flacStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _flacStream.Position;
            }
            set
            {
                _flacStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _flacStream.Read(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Thread.Sleep(100);
            if (!disposing) return;
            _flacStream.Dispose();
            if (_flacReader != null)
                _flacReader.Close();
            if (_fileStream != null)
                _fileStream.Dispose();
        }
    }
}
