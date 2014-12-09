using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public class BoolProperty : ValueProperty<bool>
    {
        public BoolProperty(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public BoolProperty()
        {

        }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsBoolean();
        }
    }
}
