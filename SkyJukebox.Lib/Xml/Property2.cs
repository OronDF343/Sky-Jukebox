using System.Xml;
using System.Xml.Schema;

namespace SkyJukebox.Lib.Xml
{
    public class Property2
    {
        public Property2() { }

        public Property2(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public virtual object Value
        {
            get { return InnerValue ?? (InnerValue = DefaultValue); }
            set { InnerValue = value; }
        }

        protected object InnerValue;

        public object DefaultValue { get; set; }

        public bool IsEditInProgress { get; protected set; }

        protected object CachedValue { get; set; }

        public virtual void ResetValue()
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

        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            Value = reader.ReadContentAsObject();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(Value);
        }
    }
}
