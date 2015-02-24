using System;
using System.Collections.Generic;

namespace SkyJukebox.Lib.Keyboard
{
    [Serializable]
    public class ToggleKeyCommand : KeyCommand
    {
        public ToggleKeyCommand() { }

        public ToggleKeyCommand(IDictionary<string, object> keyDownActions)
            : base(keyDownActions) { }

        public ToggleKeyCommand(IDictionary<string, object> keyDownActions, IDictionary<string, object> keyUpActions)
            : base(keyDownActions, keyUpActions) { }

        private bool _toggled;

        public override void OnKeyDown()
        {
            if (_toggled)
                base.OnKeyUp();
            else
                base.OnKeyDown();
            _toggled = !_toggled;
        }

        public override void OnKeyUp() { }
    }
}
