using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using SkyJukebox.Lib.Collections;

namespace SkyJukebox.Lib.Xml
{
    public class NestedProperty2 : Property2
    {
        public NestedProperty2()
        {
            DefaultValue = new ObservableDictionary<string, Property2>();
            CachedValue = new ObservableDictionary<string, Property2>();
        }

        public NestedProperty2(object defaultValue) : this()
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get 
            { 
                if (InnerValue == null) ResetValue();
                return InnerValue;
            }
            set { InnerValue = value; }
        }

        public override void ResetValue()
        {
            ValueAsDictionary.Clear();
            foreach (KeyValuePair<string, Property2> p in DefaultValueAsDictionary)
                ValueAsDictionary.Add(p.Key, p.Value);
        }

        public override void BeginEdit()
        {
            IsEditInProgress = true;
            foreach (KeyValuePair<string, Property2> p in ValueAsDictionary)
                CachedValueAsDictionary.Add(p.Key, p.Value);
        }

        public override void SaveEdit()
        {
            IsEditInProgress = false;
            CachedValueAsDictionary.Clear();
        }

        public override void DiscardEdit()
        {
            IsEditInProgress = false;
            ValueAsDictionary.Clear();
            foreach (KeyValuePair<string, Property2> p in CachedValueAsDictionary)
                ValueAsDictionary.Add(p.Key, p.Value);
        }

        private ObservableDictionary<string, Property2> ValueAsDictionary 
        { 
            get { return ((ObservableDictionary<string, Property2>)Value); }
        }

        private ObservableDictionary<string, Property2> DefaultValueAsDictionary
        {
            get { return ((ObservableDictionary<string, Property2>)DefaultValue); }
        }

        private ObservableDictionary<string, Property2> CachedValueAsDictionary
        {
            get { return ((ObservableDictionary<string, Property2>)CachedValue); }
        } 

        public override void ReadXml(XmlReader reader)
        {
            var valueSerializer = new XmlSerializer(typeof(Property2));

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("PropertyEntry");

                var key = reader.GetAttribute("Key");

                var value = (Property2)valueSerializer.Deserialize(reader);

                ValueAsDictionary.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            var valueSerializer = new XmlSerializer(typeof(Property2));

            foreach (var key in ((ObservableDictionary<string, Property2>)Value).Keys)
            {
                writer.WriteStartElement("PropertyEntry");

                writer.WriteAttributeString("Key", key);

                var value = ValueAsDictionary[key];
                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();
            }
        }
    }
}
