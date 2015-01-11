using System.Security.Principal;
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

        public static string GetRegisteredText(string fileType, string shellKeyName, string defaultString)
        {
            // path to the registry location
            var regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            using (var key = Registry.ClassesRoot.OpenSubKey(regPath))
            {
                return key != null ? (string)key.GetValue(null) : defaultString;
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

        public static bool GetIsRegistered(string fileType, string shellKeyName)
        {
            // path to the registry location
            var regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            using (var key = Registry.ClassesRoot.OpenSubKey(regPath))
                return key != null;
        }

        public static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                if (identity == null) return false;
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
