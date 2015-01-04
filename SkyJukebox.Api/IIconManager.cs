using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using SkyJukebox.Lib.Icons;

namespace SkyJukebox.Api
{
    public interface IIconManager : INotifyCollectionChanged, IDictionary<string, IIcon>
    {
        void Replace(string key, IIcon icon);
        void SetRecolorAll(Color c);
        void ResetColorAll();
        bool LoadFromSkin(string skinId, bool initial = false);
    }
}
