using System;

namespace SkyJukebox.Lib.Plugins
{
    public class PluginInfo<TInterface, TAttribute> where TAttribute : Attribute
    {
        public TAttribute Attribute { get; private set; }
        public TInterface Instance { get; private set; }

        public PluginInfo(TInterface obj, TAttribute attr)
        {
            Attribute = attr;
            Instance = obj;
        }
    }
}
