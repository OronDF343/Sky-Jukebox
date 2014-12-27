namespace SkyJukebox.Lib.Xml
{
    public class ValueProperty2 : Property2
    {
        public override void BeginEdit()
        {
            base.BeginEdit();
            CachedValue = Value;
        }

        public override void DiscardEdit()
        {
            base.DiscardEdit();
            Value = CachedValue;
        }
    }
}
