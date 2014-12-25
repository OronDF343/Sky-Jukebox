namespace SkyJukebox.Lib.Xml
{
    public class StringProperty2 : Property2
    {
        public StringProperty2() { }
        public StringProperty2(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value { get { return string.IsNullOrEmpty((string)InnerValue) ? (InnerValue = DefaultValue) : InnerValue; } set { base.Value = value; } }
    }
}
