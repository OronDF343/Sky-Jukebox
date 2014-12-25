using System.Drawing;
using System.Globalization;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public class ColorProperty2 : Property2
    {
        public ColorProperty2() { }
        public ColorProperty2(Color defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get { return ((Color)InnerValue).IsEmpty ? (InnerValue = (Color)DefaultValue) : InnerValue; }
            set { InnerValue = value; }
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
