using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Collections
{
    [XmlRoot("Dictionary")]
    public class SerializableObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableObservableDictionary() { }
        public SerializableObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary) { }
        public SerializableObservableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer) { }
        public SerializableObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer) { }

        #region IXmlSerializable Members
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");

                reader.ReadStartElement("Key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                var value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (var key in Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                var value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
