using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;

namespace SkyJukebox.Lib.Xml
{
    public abstract class PropertyBase<T> : IProperty<T>
    {
        public abstract T Value { get; set; }
        public T DefaultValue { get; set; }

        private bool _isEditing;
        public bool IsEditInProgress
        {
            get
            {
                return _isEditing;
            }
            private set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }

        protected T CachedValue { get; private set; }

        public void ResetValue()
        {
            Value = DefaultValue;
        }

        public virtual void BeginEdit()
        {
            IsEditInProgress = true;
            CachedValue = Value;
        }

        public virtual void SaveEdit()
        {
            IsEditInProgress = false;
        }

        public virtual void DiscardEdit()
        {
            IsEditInProgress = false;
            Value = CachedValue;
        }

        public static implicit operator T(PropertyBase<T> prop)
        {
            return prop.Value;
        }

        public virtual XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public abstract void ReadXml(XmlReader reader);

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
