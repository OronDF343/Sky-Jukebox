using System.Xml;

namespace SkyJukebox.CoreApi.Xml
{
    public class StringProperty : ReferenceProperty<string>
    {
        public StringProperty(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public StringProperty()
        {

        }

        public override string Value { get { return string.IsNullOrEmpty(InnerValue) ? (InnerValue = DefaultValue) : InnerValue; } set { InnerValue = value; } }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsString();
        }
    }
}
