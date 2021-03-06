﻿using System;

using NAudio.Wave;

namespace SkyJukebox.NAudioFramework
{
    /// <summary>
    /// Very simple sample provider supporting adjustable balance
    /// </summary>
    public class BalanceSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;

        /// <summary>
        /// Initializes a new instance of BalanceSampleProvider
        /// </summary>
        /// <param name="source">Source Sample Provider</param>
        public BalanceSampleProvider(ISampleProvider source)
        {
            if (source.WaveFormat.Channels != 2)
                throw new InvalidOperationException("Input wave format must be stereo!");
            _source = source;
            LeftVolume = 1.0f;
            RightVolume = 1.0f;
        }

        /// <summary>
        /// WaveFormat
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return _source.WaveFormat; }
        }

        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="sampleCount">Number of samples desired</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead;
            try { samplesRead = _source.Read(buffer, offset, sampleCount); }
            catch
            {
                return 0;
            }
            if (RightVolume == 1.0f && LeftVolume == 1.0f) return samplesRead;
            for (int n = 0; n < sampleCount; n += 2)
            {
                buffer[offset + n] *= LeftVolume;
                buffer[offset + n + 1] *= RightVolume;
            }
            return samplesRead;
        }

        /// <summary>
        /// Allows adjusting the left channel volume
        /// </summary>
        public float LeftVolume { get; set; }

        /// <summary>
        /// Allows adjusting the right channel volume
        /// </summary>
        public float RightVolume { get; set; }

        /// <summary>
        /// Legacy feature. -1 to 1.
        /// </summary>
        public float Pan
        {
            set
            {
                if (value > 0) LeftVolume -= value;
                else if (value < 0) RightVolume += value; 
                else LeftVolume = RightVolume = 1.0f;
            }
        }
    }
}
