namespace SkyJukebox.Lib.Xml
{
    public sealed class StringProperty : ValueProperty
    {
        public StringProperty() { }
        public StringProperty(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get { return string.IsNullOrEmpty((string)InnerValue) ? (InnerValue = DefaultValue) : InnerValue; }
            set { base.Value = value; }
        }
    }
}
