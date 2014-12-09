using System.Reflection;

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
    }
}
