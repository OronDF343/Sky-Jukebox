using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;

namespace SkyJukebox.Core.Icons
{
    public sealed class IconManager : INotifyCollectionChanged
    {
        #region Singleton
        private IconManager()
        {
            _iconRegistry = new Dictionary<string, IconBase>();
        }

        private static IconManager _instance;
        public static IconManager Instance { get { return _instance ?? (_instance = new IconManager()); } }
        #endregion

        private readonly Dictionary<string, IconBase> _iconRegistry;

        public void RegisterIcon(string key, IconBase icon)
        {
            _iconRegistry.Add(key, icon);
        }

        public IconBase GetIcon(string key)
        {
            return _iconRegistry[key];
        }

        public IconBase this[string key]
        {
            get { return GetIcon(key); }
            set { ReplaceIcon(key, value); }
        }

        public void ReplaceIcon(string key, IconBase icon)
        {
            _iconRegistry.Remove(key);
            _iconRegistry.Add(key, icon);
        }

        public bool RemoveIcon(string key)
        {
            return _iconRegistry.Remove(key);
        }

        public Color CurrentColor;
        public void SetRecolorAll(Color c)
        {
            foreach (var icon in _iconRegistry)
                icon.Value.SetRecolor(c);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ResetColorAll()
        {
            foreach (var icon in _iconRegistry)
                icon.Value.ResetColor();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private string _loadedSkinName;
        public void LoadFromSkin(Skin skin, bool initial = false)
        {
            _loadedSkinName = skin.Name;
            foreach (var ie in skin.IconEntries)
            {
                if (initial)
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public bool LoadFromSkin(string skinId, bool initial = false)
        {
            Skin skin;
            if (!SkinManager.Instance.SkinRegistry.TryGetValue(skinId, out skin))
                return false;
            _loadedSkinName = skin.Name;
            foreach (var ie in skin.IconEntries)
            {
                if (initial)
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }
    }
}
