using SkyJukebox.Lib.Extensions;

namespace SkyJukebox.Api
{
    [ExtensionContract("1.0.0.0", "1.0.0.0")]
    public interface IExtension
    {
        // display name
        string Name { get; }
        // display description
        string Description { get; }
        /// <summary>
        /// The name of the author of this extension.
        /// </summary>
        string Author { get; }
        /// <summary>
        /// A link to the homepage of this extension.
        /// </summary>
        string Url { get; }
            
        /// <summary>
        /// Will be called after loading the managers.
        /// </summary>
        /// <param name="contract"></param>
        void Init(IExtensionAccess contract);

        /// <summary>
        /// Will be called when the GUI is ready.
        /// </summary>
        void InitGui();

        /// <summary>
        /// Will be called when Sky Jukebox is closed.
        /// </summary>
        void Unload();
    }
}
