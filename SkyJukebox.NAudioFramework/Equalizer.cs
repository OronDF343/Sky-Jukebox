using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using NAudio.Dsp;
using NAudio.Wave;
using SkyJukebox.Api.Playback;

// Modified code from NAudioWpfDemo
namespace SkyJukebox.NAudioFramework
{
    public class Equalizer : ISampleProvider
    {
        private readonly ISampleProvider _sourceProvider;
        private readonly ObservableCollection<IEqualizerBand> _bands;
        private readonly List<BiQuadFilter[]> _filters;
        private readonly int _channels;
        private bool _updated;
        private object _lockObj;

        public bool Enabled { get; set; }

        public Equalizer(ISampleProvider sourceProvider, ObservableCollection<IEqualizerBand> bands)
        {
            _sourceProvider = sourceProvider;
            _bands = bands;
            _channels = sourceProvider.WaveFormat.Channels;
            _lockObj = new object();
            foreach (IEqualizerBand band in _bands) band.PropertyChanged += EqualizerBandPropertyChanged;
            _bands.CollectionChanged += BandsOnCollectionChanged;
            _filters = new List<BiQuadFilter[]>();
            CreateFilters();
        }

        private void BandsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IEqualizerBand item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= EqualizerBandPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IEqualizerBand item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += EqualizerBandPropertyChanged;
                }
            }  
            Update();
        }

        private void EqualizerBandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        private void CreateFilters()
        {
            lock (_lockObj)
            {
                _filters.Clear();
                foreach (var band in _bands)
                {
                    var filter = new BiQuadFilter[_channels];
                    for (var n = 0; n < _channels; n++)
                    {
                        filter[n] = BiQuadFilter.PeakingEQ(_sourceProvider.WaveFormat.SampleRate, band.Frequency,
                                                           band.Bandwidth, band.Gain);
                    }
                    _filters.Add(filter);
                }
            }
        }

        public void Update()
        {
            _updated = true;
            CreateFilters();
        }

        public WaveFormat WaveFormat
        {
            get { return _sourceProvider.WaveFormat; }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _sourceProvider.Read(buffer, offset, count);
            if (!Enabled) return samplesRead;

            if (_updated)
            {
                CreateFilters();
                _updated = false;
            }

            lock (_lockObj)
            {
                for (var n = 0; n < samplesRead; n++)
                {
                    var ch = n % _channels;

                    for (var band = 0; band < _bands.Count; band++)
                    {
                        buffer[offset + n] = _filters[band][ch].Transform(buffer[offset + n]);
                    }
                }
            }
            return samplesRead;
        }
    }
}