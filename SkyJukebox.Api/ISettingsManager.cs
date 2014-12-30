using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using SkyJukebox.Lib.Xml;

namespace SkyJukebox.Api
{
    public interface ISettingsManager : INotifyCollectionChanged, IDictionary<string, Property>, IXmlSerializable
    {
        string Path { get; }
        bool IsGlobalEditInProgress { get; }
        void ResetAll();
        void BeginEditAll();
        void SaveEditAll();
        void DiscardEditAll();
    }
}
