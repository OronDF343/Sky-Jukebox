namespace SkyJukebox.Xml
{
    public abstract class ReferenceProperty<T> : PropertyBase<T> where T : class 
    {
        public override T Value { get { return InnerValue ?? (InnerValue = DefaultValue); } set { InnerValue = value; } }
        protected T InnerValue;
    }
}
