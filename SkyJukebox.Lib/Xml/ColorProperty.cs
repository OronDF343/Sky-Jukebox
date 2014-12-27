using System.Drawing;
using System.Globalization;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public sealed class ColorProperty : ValueProperty
    {
        public ColorProperty() { }
        public ColorProperty(Color defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get { return InnerValue == null || ((Color)InnerValue).IsEmpty ? (InnerValue = DefaultValue) : InnerValue; }
            set
            {
                InnerValue = value;
                OnValueChanged();
            }
        }

        private int ValueInt
        {
            get { return ((Color)Value).ToArgb(); }
            set { Value = Color.FromArgb(value); }
        }


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
