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

        
        // In short:
        // (?<=(?<![\$])(\$\$)*)    matches only if even number of $ preceeds this
        // \$\w{2}                  what we're looking for
        // (\(.*?(?<!\$)\))?        optional: ( followed by: anyting (lazy) followed by: ) not preceeded by $
        //
        // In very long:
        //  (?<=                before the requested string, make sure that:
        //          (?<![\$])           there must not be $ before this [remember that we are moving backwards, we need to make sure we count all the consecutive $ as far back as possible]
        //      (\$\$)*             there are zero or more pairs of $ 
        //  )                   [after this there can only be 0 or 1 $ left. 0 means there is nothing unescaped here]
        //
        //  \$\w{2}             find $ followed by 2 alphanumeric characters [this is what we are looking for]
        //
        //  (                   [optional, must be right after what we found]
        //      \(                  find ( 
        //      .*?                 followed by zero or more of any character 
        //          (?<!\$)             the last of these characters must not be $ [so the ) is not escaped. looking backwards because we want to allow empty ()]
        //      \)                  followed by )
        //  )?                  this is optional, only include this if it exists
        // this will give us $xx or $xx(...)
        private const string RegexString = @"(?<=(?<![\$])(\$\$)*)\$\w{2}(\(.*?(?<!\$)\))?";


        private static string ParseWithFallback(IMusicInfo m, string h)
        {
            // we have $xx or $xx(...)
            // get the value of $xx:
            var s = FormatElements[h.Substring(0, 3)](m);

            // if there is no need to fallback, return the value
            if (!string.IsNullOrWhiteSpace(s)) return s;

            // we need to fallback
            // if we don't have a fallback, return a default string
            // if we do, lets use our regex to parse it, recursively calling this method where needed
            return h.Length < 5 ? "[Unknown]" : Regex.Replace(h.Substring(3, h.Length - 4), RegexString, match => ParseWithFallback(m, match.Value));
        }

        public static string FormatHeader(IMusicInfo m, string h)
        {
            try
            {
                // use our regex to parse this, call ParseWithFallback to evaluate variables and fallback
                var r = Regex.Replace(h, RegexString, match => ParseWithFallback(m, match.Value));
                // unescape the remaining characters
                // find:
                // \$       find $
                // (.)      followed by one character (any) -> to capture group 1 [that is what the () are for]
                // replace:
                // $1       replace with capture group 1
                return Regex.Replace(r, @"\$(.)", @"$1");
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
