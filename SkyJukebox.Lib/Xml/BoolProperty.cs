using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public sealed class BoolProperty : ValueProperty
    {
        public BoolProperty() { }

        public BoolProperty(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (bool)(InnerValue ?? (InnerValue = (bool?)DefaultValue));
            }
            set
            {
                InnerValue = (bool?)value;
                OnValueChanged();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsBoolean();
        }
    }
}
