using System;
using SkyJukebox.Api;
using SkyJukebox.Api.Playback;
using SkyJukebox.Api.Playlist;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Playback;
using SkyJukebox.Core.Playlist;
using SkyJukebox.Core.Xml;

namespace SkyJukebox
{
    public class ExtensionAccess : IExtensionAccess
    {
        public IPlaybackManager PlaybackManagerInstance { get { return PlaybackManager.Instance; } }
        public IIconManager IconManagerInstance { get { return IconManager.Instance; } }
        public ISettingsManager SettingsManagerInstance { get { return SettingsManager.Instance; } }
        public IPlaylistDataManager PlaylistDataManagerInstance { get { return PlaylistDataManager.Instance; } }
        public IInstanceManager InstanceManagerInstance { get { return InstanceManager.Instance; } }
        public void AddPluginButton(string btnId, string iconId, Action onClick, string toolTip)
        {
            InstanceManager.Instance.MiniPlayerInstance.PlWidget.AddButton(btnId, iconId, onClick, toolTip);
        }

        public void RemovePluginButton(string btnId)
        {
            InstanceManager.Instance.MiniPlayerInstance.PlWidget.RemoveButton(btnId);
        }
    }
}
