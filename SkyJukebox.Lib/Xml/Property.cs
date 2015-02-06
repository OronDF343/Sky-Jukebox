using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public class Property : INotifyPropertyChanged, IXmlSerializable
    {
        public virtual object Value
        {
            get { return InnerValue ?? (InnerValue = DefaultValue); }
            set
            {
                InnerValue = value;
                OnValueChanged();
            }
        }

        protected object InnerValue;

        public virtual object DefaultValue { get; set; }

        [XmlIgnore]
        public bool IsEditInProgress { get; protected set; }

        protected object CachedValue { get; set; }

        public virtual void ResetValue()
        {
            Value = DefaultValue;
        }

        public virtual void BeginEdit()
        {
            CheckAndSetIsEditInProgress(true);
        }

        public virtual void SaveEdit()
        {
            CheckAndSetIsEditInProgress(false);
        }

        public virtual void DiscardEdit()
        {
            CheckAndSetIsEditInProgress(false);
        }

        private void CheckAndSetIsEditInProgress(bool targetValue)
        {
            if (!IsEditInProgress && !targetValue)
                throw new InvalidOperationException("Not currently editing!");
            if (IsEditInProgress && targetValue)
                throw new InvalidOperationException("Already editing!");
            IsEditInProgress = targetValue;
        }

        public virtual void Init(object value)
        {
            Value = value;
        }

        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsObject();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnValueChanged()
        {
            OnPropertyChanged("Value");
        }
    }
}
