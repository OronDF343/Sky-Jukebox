using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using SkyJukebox.CoreApi.Playback;

namespace SkyJukebox.CoreApi.Keyboard
{
    public sealed class KeyBindingManager : IDisposable
    {
        private KeyBindingManager()
        {
            _keyboardListener = new KeyboardListener();
            KeyBindingRegistry = new List<KeyBinding>();
            _keyboardListener.KeyUp += keyboardListener_KeyUp;
            _keyboardListener.KeyDown += keyboardListener_KeyDown;
            _commandsRegistry = new Dictionary<string, Action> 
            { 
                { "PlayPauseResume", () => PlaybackManager.Instance.PlayPauseResume() },
                { "Stop", () => PlaybackManager.Instance.Stop() },
                { "Previous", () => PlaybackManager.Instance.Previous() },
                { "Next", () => PlaybackManager.Instance.Next() }
            };
        }

        private readonly Dictionary<string, Action> _commandsRegistry;

        public List<KeyBinding> KeyBindingRegistry { get; private set; }
        private readonly KeyboardListener _keyboardListener;

        private static readonly XmlSerializer MyXs = new XmlSerializer(typeof(List<KeyBinding>), new XmlRootAttribute("KeyBindingRegistry"));
        private static string _filePath;

        private static KeyBindingManager _instance;
        public static KeyBindingManager Instance { get { return _instance; } }

        public static void Init(string path)
        {
            _instance = new KeyBindingManager();
            _filePath = path;
            if (File.Exists(path))
                LoadFromXml();
        }

        private static void LoadFromXml()
        {
            try
            {
                var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var t = (List<KeyBinding>)MyXs.Deserialize(fs);
                fs.Close();
                Instance.KeyBindingRegistry.AddRange(t);
            }
            catch
            {
            }
        }

        public static void SaveToXml()
        {
            if (_filePath == null) return;
            if (!File.Exists(_filePath))
            {
                // work around bug with File.Create()
                var cs = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                cs.Close();
            }
            var fs = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write);
            MyXs.Serialize(fs, Instance.KeyBindingRegistry);
            fs.Close();
        }

        private readonly List<KeyBinding> _lastBindings = new List<KeyBinding>();
        private readonly HashSet<Key> _lastKeys = new HashSet<Key>();

        private void keyboardListener_KeyUp(object sender, RawKeyEventArgs e)
        {
            var kb = _lastBindings.FirstOrDefault(k => k.Gesture.SetEquals(_lastKeys));
            if (kb != default(KeyBinding))
            {
                foreach (var a in kb.KeyUpCommands)
                    _commandsRegistry[a]();
                _lastBindings.Remove(kb);
            }

            _lastKeys.Remove(e.Key);
        }

        private void keyboardListener_KeyDown(object sender, RawKeyEventArgs e)
        {
            _lastKeys.Add(e.Key);

            var kb = KeyBindingRegistry.FirstOrDefault(k => k.Gesture.SetEquals(_lastKeys));
            if (kb != default(KeyBinding))
            {
                _lastBindings.Add(kb);
                foreach (var a in kb.KeyDownCommands)
                    _commandsRegistry[a]();
            }
        }

        public void Dispose()
        {
            _keyboardListener.Dispose();
        }
    }
}
