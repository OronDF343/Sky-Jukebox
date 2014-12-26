using System.Windows;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public class PointProperty2 : ValueProperty2<Point>
    {
        public PointProperty2() { }
        public PointProperty2(Point defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("X");
            var x = reader.ReadContentAsDouble();
            reader.ReadEndElement();
            reader.ReadStartElement("Y");
            var y = reader.ReadContentAsDouble();
            reader.ReadEndElement();
            Value = new Point(x, y);
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
