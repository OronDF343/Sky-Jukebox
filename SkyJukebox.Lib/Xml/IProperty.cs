using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public interface IProperty<T> : IXmlSerializable
    {
        T Value { get; set; }
        T DefaultValue { get; set; }

        void ResetValue();
    }
}
