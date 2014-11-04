using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SkyJukebox.Core.Xml
{
    public abstract class PropertyBase<T> : IXmlSerializable
    {
        public abstract T Value { get; set; }
        public T DefaultValue { get; set; }

        public static implicit operator T(PropertyBase<T> tp)
        {
            return tp.Value;
        }

        public virtual void ResetValue()
        {
            Value = DefaultValue;
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
    }
}
