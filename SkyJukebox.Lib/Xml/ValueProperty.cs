namespace SkyJukebox.Lib.Xml
{
    public class ValueProperty : Property
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
