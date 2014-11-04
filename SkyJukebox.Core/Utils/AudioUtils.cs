using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace SkyJukebox.Core.Utils
{
    public static class AudioUtils
    {
        public static Dictionary<Guid, string> GetOutputDevicesInfo()
        {
            return DirectSoundOut.Devices.ToDictionary(d => d.Guid, d => d.Description);
        }
    }
}
