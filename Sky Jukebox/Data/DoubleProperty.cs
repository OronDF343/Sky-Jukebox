using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkyJukebox.Data
{
    [Serializable]
    public class DoubleProperty : IXmlSerializable
    {
        public DoubleProperty(double defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public DoubleProperty()
        {

        }

        [XmlIgnore]
        public double Value { get { return (_innerValue ?? (_innerValue = DefaultValue)).Value; } set { _innerValue = value; } }
        [XmlAttribute("Value")]
        private double? _innerValue;
        [XmlIgnore]
        public double DefaultValue { get; set; }

        public static implicit operator double(DoubleProperty bp)
        {
            return bp.Value;
        }

        public void ResetValue()
        {
            _innerValue = DefaultValue;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            Value = reader.ReadElementContentAsDouble();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteValue(Value);
        }
    }
}
