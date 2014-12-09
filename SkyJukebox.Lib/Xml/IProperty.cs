using System.ComponentModel;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public interface IProperty<T> : IProperty
    {
        T Value { get; set; }
        T DefaultValue { get; set; }
    }

    public interface IProperty : IXmlSerializable, INotifyPropertyChanged
    {
        bool IsEditInProgress { get; }

        void ResetValue();
        void BeginEdit();
        void SaveEdit();
        void DiscardEdit();
    }
}
