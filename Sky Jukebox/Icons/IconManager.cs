using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.Icons
{
    public sealed class IconManager
    {
        #region Singleton
        private IconManager()
        {
            _iconRegistry = new Dictionary<string, Icon>();
            LoadSkin(Skin.DefaultSkin, true);
        }

        private static IconManager _instance;
        public static IconManager Instance { get { return _instance ?? (_instance = new IconManager()); } }
        #endregion

        private readonly Dictionary<string, Icon> _iconRegistry;

        public void RegisterIcon(string key, Icon icon)
        {
            _iconRegistry.Add(key, icon);
        }

        public Icon GetIcon(string key)
        {
            return _iconRegistry[key];
        }

        public void ReplaceIcon(string key, Icon icon)
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
        public void LoadSkin(Skin skin, bool initial = false)
        {
            _loadedSkinName = skin.Name;
            foreach (var ie in skin.IconEntries)
            {
                if (initial)
                    RegisterIcon(ie.Key, skin.IsEmbedded ? (Icon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
                else
                    ReplaceIcon(ie.Key, skin.IsEmbedded ? (Icon)new EmbeddedPngIcon(ie.Path) : new FileIcon(ie.Path));
            }
        }
    }
}
