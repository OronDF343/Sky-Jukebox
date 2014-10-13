using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkyJukebox.Icons
{
    public class SkinManager
    {
        #region Singleton
        private SkinManager()
        {
            SkinRegistry = new Dictionary<string, Skin> { { Skin.DefaultSkin.Name, Skin.DefaultSkin } };
        }

        private static SkinManager _instance;
        public static SkinManager Instance { get { return _instance ?? (_instance = new SkinManager()); } }
        #endregion

        public readonly Dictionary<string, Skin> SkinRegistry;

        public void LoadAllSkins(string dir)
        {
            var di = new DirectoryInfo(dir);
            if (!di.Exists) throw new DirectoryNotFoundException("Directory not found: " + dir);
            foreach (var f in di.GetFiles())
            {
                var s = LoadSkin(f.FullName);
                SkinRegistry.Add(s.Name, s);
            }
        }

        private static readonly XmlSerializer MyXs = new XmlSerializer(typeof(Skin));
        public static Skin LoadSkin(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var t = (Skin)MyXs.Deserialize(fs);
            fs.Close();
            return t;
        }
        public static void SaveSkin(string path, Skin skin)
        {
            if (!File.Exists(path)) File.Create(path);
            var fs = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            MyXs.Serialize(fs, skin);
            fs.Close();
        }
    }
}
