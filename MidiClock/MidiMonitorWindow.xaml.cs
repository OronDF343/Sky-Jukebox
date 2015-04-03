using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace MidiClock
{
    /// <summary>
    /// Interaction logic for MidiMonitorWindow.xaml
    /// </summary>
    public partial class MidiMonitorWindow : INotifyPropertyChanged
    {
        public MidiPlayer MidiPlayer { get; private set; }

        public MidiMonitorWindow(MidiPlayer mp)
        {
            MidiPlayer = mp;
            InitializeComponent();
            _timer.Tick += (sender, args) => OnPropertyChanged("MidiPlayer");
            _timer.IsEnabled = true;
        }

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5), IsEnabled = false };

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
