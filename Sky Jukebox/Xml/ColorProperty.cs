using System.Drawing;
using System.Globalization;
using System.Xml;

namespace SkyJukebox.Xml
{
    public class ColorProperty : PropertyBase<Color>
    {
        public ColorProperty(Color defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public ColorProperty()
        {

        }

        public override Color Value { get { return _innerValue.IsEmpty ? (_innerValue = DefaultValue) : _innerValue; } set { _innerValue = value; } }
        private Color _innerValue;

        private int ValueInt
        {
            get { return Value.ToArgb(); }
            set { Value = Color.FromArgb(value); }
        }

        public byte A { get { return Value.A; } }
        public byte R { get { return Value.R; } }
        public byte G { get { return Value.G; } }
        public byte B { get { return Value.B; } }

        public override void ReadXml(XmlReader reader)
        {
            ValueInt = int.Parse(reader.ReadElementContentAsString(), NumberStyles.HexNumber);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(ValueInt.ToString("X8"));
        }
    }
}
