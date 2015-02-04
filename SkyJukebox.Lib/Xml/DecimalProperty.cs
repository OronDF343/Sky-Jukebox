using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public sealed class DecimalProperty : ValueProperty
    {
        public DecimalProperty() { }

        public DecimalProperty(decimal defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (decimal)(InnerValue ?? (InnerValue = (decimal?)DefaultValue));
            }
            set
            {
                InnerValue = (decimal?)value;
                OnValueChanged();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsDecimal();
        }
    }
}
