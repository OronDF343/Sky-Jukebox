using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            {"$T", info => TestString(info.Tag.Title, info.FileName)},
            {"$P1", info => TestString(info.Tag.FirstPerformer, "Unknown Performer")},
            {"$PJ", info => TestString(info.Tag.JoinedPerformers, "Unknown Performer")},
            {"$A1", info => TestString(info.Tag.FirstAlbumArtist, "Unknown Album Artist")},
            {"$AJ", info => TestString(info.Tag.JoinedAlbumArtists, "Unknown Album Artist")},
            {"$L", info => TestString(info.Tag.Album, "Unknown Album")},
            {"$N", info => info.Tag.Track.ToString(CultureInfo.InvariantCulture)},
            {"$G1", info => TestString(info.Tag.FirstGenre, "Unknown Genre")},
            {"$GJ", info => TestString(info.Tag.JoinedGenres, "Unknown Genre")},
            {"$Y", info => TestString(info.Tag.Year.ToString(CultureInfo.InvariantCulture), "Unknown Year")},
            {"$D", info => info.Duration.ToString()},
            {"$E", info => info.Extension},
            {"$B", info => info.Bitrate.ToString(CultureInfo.InvariantCulture)},
        };
        private static readonly Dictionary<string, string> EscapeSequences = new Dictionary<string, string>
        {
            {"$$", "$"},
        };

        public static string TestString(string s, string fallback)
        {
            return string.IsNullOrWhiteSpace(s) ? fallback : s;
        }

        public static string FormatHeader(IMusicInfo m, string h)
        {
            var r = FormatElements.Aggregate(h, (current, p) => current.Replace(p.Key, p.Value(m)));
            return EscapeSequences.Aggregate(r, (current, p) => current.Replace(p.Key, p.Value));
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
