using System.Collections.Specialized;
using System.Drawing;
using SkyJukebox.Api;
using SkyJukebox.Lib.Collections;
using SkyJukebox.Lib.Icons;

namespace SkyJukebox.Core.Icons
{
    public sealed class IconManager : ObservableDictionary<string, IIcon>, IIconManager
    {
        #region Singleton
        private IconManager() { }

        private static IconManager _instance;
        public static IconManager Instance { get { return _instance ?? (_instance = new IconManager()); } }
        #endregion

        public new IIcon this[string key]
        {
            get { return base[key]; }
            set { Replace(key, value); }
        }

        public void Replace(string key, IIcon icon)
        {
            Remove(key);
            Add(key, icon);
        }

        public void SetRecolorAll(Color c)
        {
            foreach (var icon in Values)
                icon.SetRecolor(c);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ResetColorAll()
        {
            foreach (var icon in Values)
                icon.ResetColor();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private string _loadedSkinName;
        public void LoadFromSkin(Skin skin, bool initial = false)
        {
            _loadedSkinName = skin.Name;
            foreach (var ie in skin.IconEntries)
            {
                if (initial)
                    Add(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    Replace(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public bool LoadFromSkin(string skinId, bool initial = false)
        {
            Skin skin;
            if (!SkinManager.Instance.SkinRegistry.TryGetValue(skinId, out skin))
                return false;
            LoadFromSkin(skin, initial);
            return true;
        }
    }
}
