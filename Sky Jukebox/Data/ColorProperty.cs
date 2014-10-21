using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkyJukebox.Data
{
    [Serializable]
    public class ColorProperty : IXmlSerializable
    {
        public ColorProperty(Color defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public ColorProperty()
        {

        }

        [XmlIgnore]
        public Color Value { get { return _innerValue == Color.Empty ? (_innerValue = DefaultValue) : _innerValue; } set { _innerValue = value; } }
        [XmlIgnore]
        private Color _innerValue;
        [XmlAttribute("Value")]
        public string ValueHtml
        {
            get { return ColorTranslator.ToHtml(Value); }
            set { Value = ColorTranslator.FromHtml(value); }
        }
        [XmlIgnore]
        public Color DefaultValue { get; private set; }

        [XmlIgnore]
        public byte A { get { return Value.A; } }
        [XmlIgnore]
        public byte R { get { return Value.R; } }
        [XmlIgnore]
        public byte G { get { return Value.G; } }
        [XmlIgnore]
        public byte B { get { return Value.B; } }

        public static implicit operator Color(ColorProperty cp)
        {
            return cp.Value;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            ValueHtml = reader.ReadElementContentAsString();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteValue(ValueHtml);
        }
    }
}
