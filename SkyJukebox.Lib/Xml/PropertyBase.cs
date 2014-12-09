using System;
using System.Xml;
using System.Xml.Schema;

namespace SkyJukebox.Lib.Xml
{
    public abstract class PropertyBase<T> : IProperty<T>
    {
        public abstract T Value { get; set; }
        public T DefaultValue { get; set; }

        public void ResetValue()
        {
            Value = DefaultValue;
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
    }
}
