using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace SkyJukebox.NAudioFramework.Codecs
{
    public class CoreCodec : ICodec
    {
        public string Name { get { return typeof(AudioFileReader).FullName; } }
        public WaveStream CreateWaveStream(string path)
        {
            return new AudioFileReader(path);
        }

        private IEnumerable<string> BaseExts { get { return new[] { "mp3", "wav", "aiff", "wma" }; } }
        private IEnumerable<string> Win7Exts { get { return new[] { "m4a", "aac", "adts" }; } }
        private IEnumerable<string> Win8Exts { get { return new[] { "ac3" }; } }

        public IEnumerable<string> Extensions
        {
            get
            {
                return IsWindows8OrHigher
                           ? BaseExts.Concat(Win7Exts).Concat(Win8Exts)
                           : IsWindows7OrHigher ? BaseExts.Concat(Win7Exts) : BaseExts;
            }
        }


        private static bool? _isWin8;
        public static bool IsWindows8OrHigher
        {
            get { return (_isWin8 ?? (_isWin8 = Environment.OSVersion.Version >= Version.Parse("6.2.0.0"))).Value; }
        }

        private static bool? _isWin7;
        public static bool IsWindows7OrHigher
        {
            get { return (_isWin7 ?? (_isWin7 = Environment.OSVersion.Version >= Version.Parse("6.1.0.0"))).Value; }
        }
    }
}
