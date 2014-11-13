using System;

namespace SkyJukebox.Lib.Extensions
{
    public class ExtensionInfo<TInterface, TAttribute> where TAttribute : Attribute
    {
        public TAttribute Attribute { get; private set; }
        public TInterface Instance { get; private set; }

        public ExtensionInfo(TInterface obj, TAttribute attr)
        {
            Attribute = attr;
            Instance = obj;
        }
    }
}
