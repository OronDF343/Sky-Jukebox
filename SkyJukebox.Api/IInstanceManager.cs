using System.Collections.Generic;
using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Api
{
    public interface IInstanceManager
    {
        string ExeDir { get; }
        string ExeFilePath { get; }
        string UserDataDir { get; }
        string SettingsFileName { get; }
        string SettingsFilePath { get; }
        string SkinsFolderName { get; }
        string SkinsFolderPath { get; }
        string KeyConfigFileName { get; }
        string KeyConfigFilePath { get; }
        string ProgId { get; }
        IEnumerable<ExtensionInfo<IExtension>> LoadedExtensions { get; }
        List<string> CommmandLineArgs { get; }
        string CurrentReleaseTag { get; }
    }
}
