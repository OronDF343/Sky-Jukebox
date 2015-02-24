using System;
using System.Collections.Generic;
using SkyJukebox.Lib.Collections;

namespace SkyJukebox.Lib.Keyboard
{
    [Serializable]
    public class KeyCommand
    {
        public KeyCommand()
        {
            KeyDownActions = new SerializableDictionary<string, object>();
            KeyUpActions = new SerializableDictionary<string, object>();
        }

        public KeyCommand(IDictionary<string, object> keyDownActions)
        {
            KeyDownActions = new SerializableDictionary<string, object>(keyDownActions);
            KeyUpActions = new SerializableDictionary<string, object>();
        }

        public KeyCommand(IDictionary<string, object> keyDownActions, IDictionary<string, object> keyUpActions)
        {
            KeyDownActions = new SerializableDictionary<string, object>(keyDownActions);
            KeyUpActions = new SerializableDictionary<string, object>(keyUpActions);
        }

        public SerializableDictionary<string, object> KeyDownActions { get; set; }

        public SerializableDictionary<string, object> KeyUpActions { get; set; }


        public virtual void OnKeyDown()
        {
            foreach (var a in KeyDownActions)
                KeyBindingManager.Instance.Actions[a.Key](a.Value);
        }

        public virtual void OnKeyUp()
        {
            foreach (var a in KeyUpActions)
                KeyBindingManager.Instance.Actions[a.Key](a.Value);
        }
    }
}
