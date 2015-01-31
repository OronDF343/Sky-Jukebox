using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace SkyJukebox.Lib
{
    public static class Utils
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
                    tmp = Directory.GetFiles(path);
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
                    tmp = Directory.GetDirectories(path);
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
    }
}
