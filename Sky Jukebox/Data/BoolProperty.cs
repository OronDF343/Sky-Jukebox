using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkyJukebox
{
    [Serializable]
    public class BoolProperty : IXmlSerializable
    {
        public BoolProperty(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public BoolProperty()
        {

        }

        [XmlIgnore]
        public bool Value { get { return (bool)(_innerValue ?? (_innerValue = DefaultValue)); } set { _innerValue = value; } }
        [XmlAttribute("Value")]
        private bool? _innerValue;
        [XmlIgnore]
        public bool DefaultValue { get; private set; }

        public static implicit operator bool(BoolProperty bp)
        {
            return bp.Value;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            Value = reader.ReadElementContentAsBoolean();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteValue(Value);
        }
    }
}
