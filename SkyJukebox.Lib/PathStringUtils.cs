using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SkyJukebox.Lib
{
    public static class PathStringUtils
    {
        /// <summary>
        /// Substring to end index instead of length.
        /// </summary>
        /// <param name="s">Input string</param>
        /// <param name="startIndex">Start index, inclusive</param>
        /// <param name="endIndex">End index, exclusive</param>
        /// <returns></returns>
        public static string SubstringRange(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string GetExt(this string path)
        {
            return path.SubstringRange(path.LastIndexOf('.') + 1, path.Length).ToLowerInvariant();
        }

        public static string GetExePath()
        {
            var epath = Assembly.GetExecutingAssembly().Location;
            return epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
        }

        public static IEnumerable<string> GetFiles(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            string[] tmp;
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    tmp = DirectoryEx.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }

                foreach (var t in tmp)
                    yield return t;

                try
                {
                    tmp = DirectoryEx.GetDirectories(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }

                foreach (var subDir in tmp)
                    queue.Enqueue(subDir);
            }
        }

        public static IEnumerable<FileInfoEx> EnumerateFilesEx(this DirectoryInfoEx path)
        {
            var queue = new Queue<DirectoryInfoEx>();
            queue.Enqueue(path);
            IEnumerable<FileSystemInfoEx> tmp;
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    tmp = path.GetFiles();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }

                foreach (var t in tmp)
                    yield return t as FileInfoEx;

                try
                {
                    tmp = path.GetDirectories();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }

                foreach (var subDir in tmp)
                    queue.Enqueue(subDir as DirectoryInfoEx);
            }
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
    }
}
