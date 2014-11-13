using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FlacDotNet;
using FlacDotNet.Frames;
using FlacDotNet.Meta;
using NAudio.Wave;

namespace NAudioFlacBox
{
    /// <summary>
    /// Stream for encoding/decoding WAVE data into FLAC container.
    /// </summary>
    public class FlacFileReader2 : WaveStream
    {
        IEnumerator<ArraySegment<byte>> _dataSource;
        public Metadata[] Meta { get; private set; }

        private readonly string _path;
        public FlacFileReader2(string path)
        {
            _reader = new FlacDecoder(new FileStream(_path = path, FileMode.Open, FileAccess.Read, FileShare.Read));
            Meta = _reader.ReadMetadata();
            _streamInfo = _reader.GetStreamInfo();
            _currentData = NoCurrentData;
            _dataSource = ReadFlac();
        }

        private FlacDecoder _reader;

        public override long Length
        {
            get
            {
                return _streamInfo.TotalSamples * WaveFormat.BlockAlign;
            }
        }

        public override long Position
        {
            get
            {
                long lastSampleNumber;
                lock (_repositionLock)
                {
                    lastSampleNumber = _lastSampleNumber;
                }
                return lastSampleNumber * WaveFormat.BlockAlign;
            }
            set
            {
                lock (_repositionLock)
                {
                    // Note: Adjust NAudio position to FLAC sample number (which is raw and ignores takes block align)
                    _repositionRequested = true;
                    _flacReposition = value / WaveFormat.BlockAlign;
                    _lastSampleNumber = _flacReposition;
                    // TODO: FIX ALL MESSY CODE. Do I really have to? :-(
                    if (!_wasRead) return;
                    _currentData = NoCurrentData;
                    // I tried. can't fix this yet
                    _reader.Dispose();
                    _dataSource.Dispose();
                    _reader = new FlacDecoder(new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read));
                    Meta = _reader.ReadMetadata();
                    _dataSource = ReadFlac();
                    _wasRead = false;
                }
            }
        }

        private bool _wasRead;
        public override int Read(byte[] buffer, int offset, int count)
        {
            _wasRead = true;
            if (count < 0) throw new IndexOutOfRangeException();

            if (!CanRead) throw new NotSupportedException();

            if (_currentData.Count >= count)
            {
                Array.Copy(_currentData.Array, _currentData.Offset, buffer, offset, count);
                _currentData = new ArraySegment<byte>(_currentData.Array, _currentData.Offset + count, _currentData.Count - count);
                return count;
            }
            var read = _currentData.Count;
            Array.Copy(_currentData.Array, _currentData.Offset, buffer, offset, _currentData.Count);
            _currentData = NoCurrentData;

            while (_dataSource.MoveNext())
            {
                var rest = count - read;
                if (_dataSource.Current.Count >= rest)
                {
                    Array.Copy(_dataSource.Current.Array, 0, buffer, offset + read, rest);
                    read += rest;
                    _currentData = new ArraySegment<byte>(_dataSource.Current.Array, rest, _dataSource.Current.Count - rest);
                    break;
                }
                Array.Copy(_dataSource.Current.Array, 0, buffer, offset + read, _dataSource.Current.Count);
                read += _dataSource.Current.Count;
            }
            return read;
        }

        private WaveFormat _waveFormat;

        public override WaveFormat WaveFormat
        {
            get { return _waveFormat ?? (_waveFormat = new WaveFormat(_streamInfo.SampleRate, _streamInfo.BitsPerSample, _streamInfo.Channels)); }
        }

        private static readonly ArraySegment<byte> NoCurrentData = new ArraySegment<byte>(new byte[0]);


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            // might still be reading, don't want to interrupt it and crash, so wait just a little bit
            Thread.Sleep(100);
            if (_dataSource != null)
            {
                _dataSource.Dispose();
                _dataSource = null;
            }
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        ArraySegment<byte> _currentData;
        readonly StreamInfo _streamInfo;
        long _flacReposition;

        private IEnumerator<ArraySegment<byte>> ReadFlac()
        {
            if (_streamInfo.BitsPerSample != 8 && _streamInfo.BitsPerSample != 16 && _streamInfo.BitsPerSample != 24 && _streamInfo.BitsPerSample != 32)
                throw new NotSupportedException("Unsupported bits per sample");
            if (_streamInfo.TotalSamples < 1)
                throw new ApplicationException("Total samples cannot be unknown");
            InitializeHelpers();

            int exceptionCount = 0;
            while (!_reader.IsEof())
            {
                ArraySegment<byte> current;
                try
                {
                    current = ReadFrame();
                }
                catch (Exception)
                {
                    ++exceptionCount;
                    if (exceptionCount < 4 && !_reader.IsEof())
                        continue;
                    break;
                }
                exceptionCount = 0;
                yield return current;
            }
        }

        byte[] _pcmBuffer;

        private void InitializeHelpers()
        {
            var bufferSize = _streamInfo.MaxBlockSize * ((_streamInfo.Channels * _streamInfo.BitsPerSample) >> 3);
            _pcmBuffer = new byte[bufferSize];
        }

        private long _lastSampleNumber;
        private readonly object _repositionLock = new object();
        private bool _repositionRequested;

        private ArraySegment<byte> ReadFrame()
        {
            Frame frame;
            lock (_repositionLock)
            {
                if (_repositionRequested)
                {
                    _repositionRequested = false;
                    if (!_reader.Seek(_flacReposition))
                        throw new IOException("Failed to seek!");
                    frame = _reader.GetLastFrame();
                }
                else
                    frame = _reader.ReadNextFrame();
                // Keep the current sample number for reporting purposes (See: Position property of FlacFileReader)
                if (frame != null)
                    _lastSampleNumber = frame.Header.SampleNumber;
            }
            var read = _reader.DecodeFrame(frame, null);
            Array.Copy(read.Data, _pcmBuffer, read.Length);
            return new ArraySegment<byte>(_pcmBuffer, 0, read.Length);
        }
    }
}
