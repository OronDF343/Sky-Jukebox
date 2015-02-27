using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MidiUtils.IO;
using MidiUtils.Sequencer;
using NAudio;
using NAudio.Midi;
using SkyJukebox.Api.Playback;
using MidiEvent = NAudio.Midi.MidiEvent;

namespace MidiClock
{
    public class MidiPlayer : IAudioPlayer, INotifyPropertyChanged
    {
        private static MidiOut _midi;
        private static Sequence _seq;
        private static Sequencer _sr;

        public int MidiDevice { get; set; }

        public string ExtensionId { get { return "MidiClock"; } }

        public event EventHandler PlaybackFinished;
        public event EventHandler PlaybackError;

        private string _lastPath;
        public bool Load(string path, Guid device)
        {
            try
            {
                _seq = new Sequence(path);
                _sr = new Sequencer(_seq);
                _lastPath = path;
                _sr.OnTrackEvent += sr_OnTrackEvent;
                _sr.SequenceEnd += sr_SequenceEnd;
                _sr.TempoChanged += sr_TempoChanged;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #region Event handling

        private void sr_TempoChanged(object sender, TempoChangedEventArgs e)
        {
            OnPropertyChanged("Duration");
        }

        private void sr_SequenceEnd(object sender, EventArgs eventArgs)
        {
            Unload();
            OnPlaybackFinished();
        }

        private void sr_OnTrackEvent(object sender, TrackEventArgs e)
        {
            foreach (var ev in e.Events.Where(x => x is MidiUtils.IO.MidiEvent))
            {
                // Convert the event to NAudio's system so we can get a shortMessage
                var em = (MidiUtils.IO.MidiEvent)ev;
                MidiEvent me;
                try
                {
                    switch (ev.Type)
                    {
                        case EventType.NoteOn:
                        case EventType.NoteOff:
                        case EventType.PolyphonicKeyPressure:
                            if (em.Data2 > 0 && ev.Type == EventType.NoteOn)
                            {
                                me = new NoteOnEvent(ev.Tick, em.Channel + 1, em.Data1, em.Data2, 0);
                            }
                            else
                            {
                                me = new NoteEvent(ev.Tick, em.Channel + 1, (MidiCommandCode)(int)ev.Type, em.Data1, em.Data2);
                            }
                            break;
                        case EventType.ControlChange:
                            me = new ControlChangeEvent(ev.Tick, em.Channel + 1, (MidiController)em.Data1, em.Data2);
                            break;
                        case EventType.ProgramChange:
                            me = new PatchChangeEvent(ev.Tick, em.Channel + 1, em.Data1);
                            break;
                        case EventType.ChannelPressure:
                            me = new ChannelAfterTouchEvent(ev.Tick, em.Channel + 1, em.Data1);
                            break;
                        case EventType.Pitchbend:
                            me = new PitchWheelChangeEvent(ev.Tick, em.Channel + 1, em.Data1 + (em.Data2 << 7));
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported MIDI event type: " + ev.Type);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating MIDI event: " + ex);
                    continue;
                }

                // Send the message
                try { _midi.Send(me.GetAsShortMessage()); }
                catch (MmException ex)
                {
                    if ((int)ex.Result == 67) Console.WriteLine("MIDI error: MIDI driver overloaded! Too many notes!");
                    else Console.WriteLine("Error sending MIDI message: " + ex);
                }
            }
        }

        private void OnPlaybackError(Exception ex)
        {
            var handler = PlaybackError;
            if (handler != null) handler(this, new EventArgs());
        }

        private void OnPlaybackFinished()
        {
            var handler = PlaybackFinished;
            if (handler != null) handler(this, new EventArgs());
        }
        #endregion

        public void Unload()
        {
            _seq = null;
            if (_sr != null)
            {
                _sr.OnTrackEvent -= sr_OnTrackEvent;
                _sr.SequenceEnd -= sr_SequenceEnd;
                _sr.TempoChanged -= sr_TempoChanged;
                _sr.Stop();
                _sr = null;
            }
            if (_midi != null)
            {
                _midi.Close();
                _midi = null;
            }
        }

        public void Play()
        {
            _midi = new MidiOut(MidiDevice);
            if (_sr != null)
                _sr.Start();
        }

        public void Pause()
        {
            if (_sr != null)
                _sr.Stop();
        }

        public void Resume()
        {
            if (_sr != null)
                _sr.Start();
        }

        public void Stop()
        {
            if (_sr != null)
            {
                _sr.Stop();
                Unload();
                Load(_lastPath, Guid.Empty);
            }
        }

        public decimal Volume { get; set; }

        public decimal Balance { get; set; }

        public TimeSpan Duration { get { return _sr != null && _seq != null ? TimeSpan.FromSeconds((long)(_seq.MaxTick / ((_seq.Resolution * _sr.Tempo * _sr.TempoFactor) / 60.0))) : TimeSpan.Zero; } }

        public TimeSpan Position
        {
            get { return _sr != null && _seq != null ? TimeSpan.FromSeconds((long)(_sr.Tick / ((_seq.Resolution * _sr.Tempo * _sr.TempoFactor) / 60.0))) : TimeSpan.Zero; }
            set { if (_sr != null) _sr.Tick = (long)(value.TotalSeconds * ((_seq.Resolution * _sr.Tempo * _sr.TempoFactor) / 60.0)); }
        }

        public IEnumerable<string> Extensions { get { return new[] { "mid", "midi" }; } }

        public bool IsSomethingLoaded { get { return _sr != null && _seq != null; } }
        public void Dispose()
        {
            Unload();
            if (_midi != null) _midi.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
