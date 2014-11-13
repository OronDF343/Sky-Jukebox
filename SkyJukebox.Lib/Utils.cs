namespace SkyJukebox.Lib
{
    public static class Utils
    {
        public static string SubstringRange(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string GetExt(this string path)
        {
            return path.SubstringRange(path.LastIndexOf('.') + 1, path.Length).ToLowerInvariant();
        }
    }
}
