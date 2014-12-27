namespace SkyJukebox.Lib.Xml
{
    public sealed class StringProperty2 : ValueProperty2
    {
        public StringProperty2() { }
        public StringProperty2(string defaultValue)
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
