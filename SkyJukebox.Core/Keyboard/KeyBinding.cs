using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SkyJukebox.Core.Keyboard
{
    [Serializable]
    public class KeyBinding
    {
        public KeyBinding()
        {
            Gesture = new HashSet<Key>();
            Command = new KeyCommand();
        }

        public string Name { get; set; }

        [XmlArray("Gesture")]
        [XmlArrayItem("Key")]
        public HashSet<Key> Gesture { get; set; }

        public KeyCommand Command { get; set; }
    }
}
