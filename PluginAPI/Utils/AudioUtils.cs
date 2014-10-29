using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace SkyJukebox.CoreApi.Utils
{
    public static class AudioUtils
    {
        public static Dictionary<string, Guid> GetOutputDevicesInfo()
        {
            return DirectSoundOut.Devices.ToDictionary(d => d.Description, d => d.Guid);
        }
    }
}
