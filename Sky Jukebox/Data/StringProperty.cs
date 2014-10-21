using System;
using System.Xml.Serialization;

namespace SkyJukebox.Data
{
    [Serializable]
    public class StringProperty : IXmlSerializable
    {
        public StringProperty(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public StringProperty()
        {

        }

        [XmlIgnore]
        public string Value { get { return _innerValue == "" || _innerValue == null ? (_innerValue = DefaultValue) : _innerValue; } set { _innerValue = value; } }
        [XmlAttribute("Value")]
        private string _innerValue;
        [XmlIgnore]
        public string DefaultValue { get; set; }

        public static implicit operator string(StringProperty bp)
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
            Value = reader.ReadElementContentAsString();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteValue(Value);
        }
    }
}
