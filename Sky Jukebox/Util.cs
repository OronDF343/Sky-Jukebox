using SkyJukebox.Playback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyJukebox
{
    public static class Util
    {
        public static string SubstringRange(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string FormatHeader(Music m, string h)
        {
            var artists = string.Join(", ", m.TagFile.Tag.AlbumArtists);
            var title = m.TagFile.Tag.Title;
            return (artists == "" ? "Unknown Artist" : artists) + " - " + (title ?? m.FileName);
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

        public static void SavePlaylist(Playlist data, string file, bool relative)
        {
            data.ShuffleIndex = false;
            File.WriteAllLines(file, from m in data
                                     select relative ? MakeRelativePath(file, m.FilePath) : m.FilePath);
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

        public static string GetExt(this string path)
        {
            return path.SubstringRange(path.LastIndexOf('.') + 1, path.Length).ToLowerInvariant();
        }

        public static System.Windows.Media.Color ToWpfColor(this System.Drawing.Color c)
        {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static void WriteClArgsToFile(string[] args)
        {
            try
            {
                File.WriteAllLines(Instance.ExePath + "ClArgs.txt", args);
            }
            catch
            { 
            }
        }

        public static string[] GetClArgsFromFile()
        {
            try
            {
                return File.ReadAllLines(Instance.ExePath + "ClArgs.txt");
            }
            catch (Exception)
            {
                return new string[0];
            }
        }
    }
}
