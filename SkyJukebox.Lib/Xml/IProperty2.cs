using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public interface IProperty2 : IXmlSerializable
    {
        object Value { get; set; }
        object DefaultValue { get; set; }

        bool IsEditInProgress { get; }

        void ResetValue();
        void BeginEdit();
        void SaveEdit();
        void DiscardEdit();
    }
}
