namespace SkyJukebox.Lib.Xml
{
    public sealed class DoubleProperty : ValueProperty
    {
        public DoubleProperty() { }

        public DoubleProperty(double defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (double)(InnerValue ?? (InnerValue = (double?)DefaultValue));
            }
            set
            {
                InnerValue = (double?)value;
                OnValueChanged();
            }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            Value = reader.ReadElementContentAsDouble();
        }
    }
}
