using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SkyJukebox.Keyboard
{
    [Serializable]
    public class KeyBinding
    {
        public KeyBinding()
        {
            Gesture = new HashSet<Key>();
            KeyDownCommands = new List<string>();
            KeyUpCommands = new List<string>();
        }

        public string Name { get; set; }

        [XmlArray("Gesture")]
        [XmlArrayItem("Key")]
        public HashSet<Key> Gesture { get; set; }

        [XmlArray("KeyDownCommands")]
        [XmlArrayItem("Command")]
        public List<string> KeyDownCommands { get; set; }

        [XmlArray("KeyUpCommands")]
        [XmlArrayItem("Command")]
        public List<string> KeyUpCommands { get; set; }
    }
}
