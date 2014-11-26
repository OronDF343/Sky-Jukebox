using System;

namespace SkyJukebox.Lib.Extensions
{
    public class ExtensionInfo<TInterface>
    {
        public ExtensionAttribute Attribute { get; private set; }
        public TInterface Instance { get; private set; }

        public ExtensionInfo(TInterface obj, ExtensionAttribute attr)
        {
            Attribute = attr;
            Instance = obj;
        }
    }
}
