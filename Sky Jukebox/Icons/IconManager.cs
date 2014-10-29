using System.Collections.Generic;
using System.Drawing;

namespace SkyJukebox.Icons
{
    public sealed class IconManager
    {
        #region Singleton
        private IconManager()
        {
            _iconRegistry = new Dictionary<string, IIcon>();
        }

        private static IconManager _instance;
        public static IconManager Instance { get { return _instance ?? (_instance = new IconManager()); } }
        #endregion

        private readonly Dictionary<string, IIcon> _iconRegistry;

        public void RegisterIcon(string key, IIcon icon)
        {
            _iconRegistry.Add(key, icon);
        }

        public IIcon GetIcon(string key)
        {
            return _iconRegistry[key];
        }

        public void ReplaceIcon(string key, IIcon icon)
        {
            _iconRegistry.Remove(key);
            _iconRegistry.Add(key, icon);
        }

        public bool RemoveIcon(string key)
        {
            return _iconRegistry.Remove(key);
        }

        public void SetRecolorAll(Color c)
        {
            foreach (var icon in _iconRegistry)
            {
                icon.Value.SetRecolor(c);
            }
        }

        public void ResetColorAll()
        {
            foreach (var icon in _iconRegistry)
            {
                icon.Value.ResetColor();
            }
        }

        private string _loadedSkinName;
        public void LoadFromSkin(Skin skin, bool initial = false)
        {
            _loadedSkinName = skin.Name;
            foreach (var ie in skin.IconEntries)
            {
                if (initial)
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IIcon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IIcon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
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
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IIcon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IIcon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
            return true;
        }
    }
}
