using System;
using System.IO;

namespace SkyJukebox.Utils
{
    public static class ClArgs
    {
        public static void WriteClArgsToFile(string[] args)
        {
            try
            {
                File.WriteAllLines(InstanceManager.ExeDir + "ClArgs.txt", args);
            }
            catch
            {
            }
        }

        public static string[] GetClArgsFromFile()
        {
            try
            {
                return File.ReadAllLines(InstanceManager.ExeDir + "ClArgs.txt");
            }
            catch (Exception)
            {
                return new string[0];
            }
        }
    }
}
