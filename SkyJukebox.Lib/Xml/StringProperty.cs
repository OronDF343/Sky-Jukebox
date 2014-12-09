using System.Xml;

namespace SkyJukebox.Lib.Xml
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

        public override string Value { get { return string.IsNullOrEmpty(InnerValue) ? (InnerValue = DefaultValue) : InnerValue; } set { base.Value = value; } }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsString();
        }
    }
}
