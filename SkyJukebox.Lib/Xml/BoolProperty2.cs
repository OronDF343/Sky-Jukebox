namespace SkyJukebox.Lib.Xml
{
    public sealed class BoolProperty2 : ValueProperty2
    {
        public BoolProperty2() { }

        public BoolProperty2(bool defaultValue)
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

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            Value = reader.ReadElementContentAsBoolean();
        }
    }
}
