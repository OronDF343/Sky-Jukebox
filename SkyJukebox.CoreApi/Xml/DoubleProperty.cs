using System.Xml;

namespace SkyJukebox.CoreApi.Xml
{
    public class DoubleProperty : ValueProperty<double>
    {
        public DoubleProperty(double defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public DoubleProperty()
        {

        }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsDouble();
        }
    }
}
