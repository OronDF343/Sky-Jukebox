using Microsoft.Win32;

namespace SkyJukebox.Utils
{
    // Credit to Ralph Arvesen for original code
    static class FileShellExtension
    {
        public static void Register(string fileType, string shellKeyName, string menuText, string menuCommand)
        {
            // create path to registry location
            var regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            // add context menu to the registry
            using (var key = Registry.ClassesRoot.CreateSubKey(regPath))
            {
                if (key != null) key.SetValue(null, menuText);
                // TODO: else throw
            }

            // add command that is invoked to the registry
            using (var key = Registry.ClassesRoot.CreateSubKey(
                string.Format(@"{0}\command", regPath)))
            {
                if (key != null) key.SetValue(null, menuCommand);
                // TODO: else throw
            }
        }

        public static void Unregister(string fileType, string shellKeyName)
        {
            // path to the registry location
            var regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            // remove context menu from the registry
            Registry.ClassesRoot.DeleteSubKeyTree(regPath);
        }
    }
}
