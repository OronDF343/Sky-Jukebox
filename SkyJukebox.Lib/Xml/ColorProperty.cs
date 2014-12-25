using System.Drawing;
using System.Globalization;
using System.Xml;

namespace SkyJukebox.Lib.Xml
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

        public override Color Value
        {
            get
            {
                return _innerValue.IsEmpty ? (_innerValue = DefaultValue) : _innerValue;
            }
            set
            {
                _innerValue = value;
                OnPropertyChanged();
            }
        }

        private Color _innerValue;

        private int ValueInt
        {
            get { return Value.ToArgb(); }
            set { Value = Color.FromArgb(value); }
        }

        public byte A { get { return Value.A; } set { Value = Color.FromArgb(value, Value); } }
        public byte R { get { return Value.R; } set { Value = Color.FromArgb(Value.A, value, Value.G, Value.B); } }
        public byte G { get { return Value.G; } set { Value = Color.FromArgb(Value.A, Value.R, value, Value.B); } }
        public byte B { get { return Value.B; } set { Value = Color.FromArgb(Value.A, Value.R, Value.G, value); } }

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
