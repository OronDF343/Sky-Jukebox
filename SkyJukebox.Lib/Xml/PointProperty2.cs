using System.Windows;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public sealed class PointProperty2 : ValueProperty2
    {
        public PointProperty2() { }
        public PointProperty2(Point defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (Point)(InnerValue ?? (InnerValue = (Point?)DefaultValue));
            }
            set
            {
                InnerValue = (Point?)value;
                OnValueChanged();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.ReadStartElement("X");
            var x = reader.ReadContentAsDouble();
            reader.ReadEndElement();
            reader.ReadStartElement("Y");
            var y = reader.ReadContentAsDouble();
            reader.ReadEndElement();
            Value = new Point(x, y);
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("X");
            writer.WriteValue(((Point)Value).X);
            writer.WriteEndElement();
            writer.WriteStartElement("Y");
            writer.WriteValue(((Point)Value).Y);
            writer.WriteEndElement();
        }
    }
}
