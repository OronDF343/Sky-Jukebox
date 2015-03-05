using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using SkyJukebox.Api.Playback;

namespace SkyJukebox.NAudioFramework
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow
    {
        public EqualizerWindow(NAudioFrameworkExtension ext)
        {
            _ext = ext;
            InitializeComponent();
        }

        private readonly NAudioFrameworkExtension _ext;

        public IAudioPlayer NAudioPlayer
        {
            get { return _ext.Np; }
        }

        private bool _close;

        private void EqualizerWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_close) return;
            e.Cancel = true;
            Hide();
        }

        public void CloseFinal()
        {
            _close = true;
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            foreach (IEqualizerBand b in NAudioPlayer.EqualizerBands) b.Gain = 0;
        }
    }

    public class GainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)(float)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (float)(double)value;
        }
    }

    public class TempEqBand : IEqualizerBand
    {
        private float _frequency;
        public float Frequency { get { return _frequency; } set { _frequency = value; OnPropertyChanged(); } }
        private float _gain;
        public float Gain { get { return _gain; } set { _gain = value; OnPropertyChanged(); } }
        private float _bandwidth;
        public float Bandwidth { get { return _bandwidth; } set { _bandwidth = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
