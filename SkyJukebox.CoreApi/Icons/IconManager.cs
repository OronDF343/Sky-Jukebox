using System.Collections.Generic;
using System.Drawing;
using SkyJukebox.CoreApi.Contracts;

namespace SkyJukebox.CoreApi.Icons
{
    public sealed class IconManager
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

        public void ReplaceIcon(string key, IconBase icon)
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
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
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
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (IconBase)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
            return true;
        }
    }
}
