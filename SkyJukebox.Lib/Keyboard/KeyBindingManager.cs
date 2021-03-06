﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Keyboard
{
    public sealed class KeyBindingManager : IDisposable
    {
        private KeyBindingManager()
        {
            _keyboardListener = new KeyboardListener();
            KeyBindings = new List<KeyBinding>();
            _keyboardListener.KeyUp += keyboardListener_KeyUp;
            _keyboardListener.KeyDown += keyboardListener_KeyDown;
            Actions = new Dictionary<string, Action<object>> 
            { 
                // TODO: Add more
                {"ToggleHotkeys", o => Disable = !Disable},
                {"Debug", o => Console.WriteLine("Debug key pressed!")}
            };
        }

        public Dictionary<string, Action<object>> Actions { get; private set; }

        public List<KeyBinding> KeyBindings { get; private set; }
        private readonly KeyboardListener _keyboardListener;

        private static readonly XmlSerializer MyXs = new XmlSerializer(typeof(List<KeyBinding>), new XmlRootAttribute("KeyBindings"));
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
                Instance.KeyBindings.AddRange(t);
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
            MyXs.Serialize(fs, Instance.KeyBindings);
            fs.Close();
        }

        private readonly List<KeyBinding> _lastBindings = new List<KeyBinding>();
        private readonly HashSet<Key> _lastKeys = new HashSet<Key>();

        private void keyboardListener_KeyUp(object sender, RawKeyEventArgs e)
        {
            var kb = _lastBindings.FirstOrDefault(k => k.Gesture.SetEquals(_lastKeys));
            if (kb != default(KeyBinding))
            {
                kb.Command.OnKeyUp();
                _lastBindings.Remove(kb);
            }

            _lastKeys.Remove(TranslateKey(e.Key));
        }

        private void keyboardListener_KeyDown(object sender, RawKeyEventArgs e)
        {
            _lastKeys.Add(TranslateKey(e.Key));

            var kb = KeyBindings.FirstOrDefault(k => k.Gesture.SetEquals(_lastKeys));
            if (kb != default(KeyBinding))
            {
                _lastBindings.Add(kb);
                kb.Command.OnKeyDown();
            }
        }

        private static Key TranslateKey(Key key)
        {
            if (key == Key.RightCtrl) return Key.LeftCtrl;
            if (key == Key.RightAlt) return Key.LeftAlt;
            if (key == Key.RightShift) return Key.LeftShift;
            if (key == Key.RWin) return Key.LWin;
            return key;
        }

        public bool Disable
        {
            get { return !_keyboardListener.IsEnabled; }
            set
            {
                if (value)
                {
                    _lastBindings.Clear();
                    _lastKeys.Clear();
                }
                _keyboardListener.IsEnabled = !value;
            }
        }

        public void Dispose()
        {
            _keyboardListener.Dispose();
        }
    }
}
