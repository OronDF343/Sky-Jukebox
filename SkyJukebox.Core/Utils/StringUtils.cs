using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SkyJukebox.Api;
using SkyJukebox.Lib;

namespace SkyJukebox.Core.Utils
{
    public static class StringUtils
    {
        private static readonly Dictionary<string, Func<IMusicInfo, string>> FormatElements = new Dictionary<string, Func<IMusicInfo, string>>
        {
            {"$FN", info => info.FileName},
            {"$FD", info => info.MusicFileInfo.DirectoryName},
            {"$FP", info => info.FilePath},
            {"$TI", info => info.Tag.Title},
            {"$P1", info => info.Tag.FirstPerformer},
            {"$PJ", info => info.Tag.JoinedPerformers},
            {"$A1", info => info.Tag.FirstAlbumArtist},
            {"$AJ", info => info.Tag.JoinedAlbumArtists},
            {"$AL", info => info.Tag.Album},
            {"$TN", info => info.Tag.Track.ToString(CultureInfo.InvariantCulture)},
            {"$G1", info => info.Tag.FirstGenre},
            {"$GJ", info => info.Tag.JoinedGenres},
            {"$YR", info => info.Tag.Year.ToString(CultureInfo.InvariantCulture)},
            {"$DU", info => info.Duration.ToString()},
            {"$CO", info => info.Extension},
            {"$BT", info => info.Bitrate.ToString(CultureInfo.InvariantCulture)},
        };

        private const string RegexString = @"\$\w{2}(\(.*?[^\$]\))?";

        private static string ParseWithFallback(IMusicInfo m, string h)
        {
            var s = FormatElements[h.Substring(0, 3)](m);

            if (!string.IsNullOrWhiteSpace(s)) return s;
            return h.Length < 5 ? "[Unknown]" : Regex.Replace(h.Substring(3, h.Length - 4), RegexString, match => ParseWithFallback(m, match.Value));
        }

        public static string FormatHeader(IMusicInfo m, string h)
        {
            try
            {
                return Regex.Replace(h, RegexString, match => ParseWithFallback(m, match.Value)).Replace("$$", "$").Replace("$(", "(").Replace("$)", ")");
            }
            catch
            {
                return "[Format Error]";
            }
        }

        public static IEnumerable<string> GetFiles(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                foreach (var subDir in Directory.GetDirectories(path))
                    queue.Enqueue(subDir);
                var files = Directory.GetFiles(path);
                foreach (var t in files)
                {
                    yield return t;
                }
            }
        }

        public static void SavePlaylist(IPlaylist data, string file, bool relative)
        {
            data.ShuffleIndex = false;
            File.WriteAllLines(file, from m in data
                                     select relative ? MakeRelativePath(file, m.FilePath) : m.FilePath, 
                               Encoding.UTF8);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static double Round(double src)
        {
            return double.Parse(src.ToString("F2"));
        }

        public static string GetExePath()
        {
            var epath = Assembly.GetExecutingAssembly().Location;
            return epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
        }

        public static string GetSkyJukeboxAboutString()
        {
            return "Sky Jukebox\nCopyright © 2014 OronDF343\nVersion 0.9.0 \"Modular\" Alpha4.0";
        }
    }
}
