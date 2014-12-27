using System;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public sealed class GuidProperty : ValueProperty
    {
        public GuidProperty() { }
        public GuidProperty(Guid defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (Guid)(InnerValue ?? (InnerValue = (Guid?)DefaultValue));
            }
            set
            {
                InnerValue = (Guid?)value;
                OnValueChanged();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            var s = reader.ReadElementContentAsString();
            Value = new Guid(s);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Value.ToString());
        }
    }
}
