using System.Collections.Specialized;
using System.Xml;
using SkyJukebox.Lib.Collections;

namespace SkyJukebox.Lib.Xml
{
    public sealed class NestedProperty2 : Property2
    {
        public NestedProperty2()
        {
            InnerValue = new ObservableDictionary<string, Property2>();
            CachedValue = new ObservableDictionary<string, Property2>();
            (InnerValueAsDictionary as INotifyCollectionChanged).CollectionChanged += InnerValue_CollectionChanged;
        }

        public NestedProperty2(ObservableDictionary<string, Property2> defaultValue)
            : this()
        {
            InnerValue = defaultValue;
        }

        private void InnerValue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnValueChanged();
        }

        public override object Value
        {
            get { return InnerValue; }
            set
            {
                (InnerValueAsDictionary as INotifyCollectionChanged).CollectionChanged -=
                    InnerValue_CollectionChanged;
                InnerValue = value;
                (InnerValueAsDictionary as INotifyCollectionChanged).CollectionChanged +=
                    InnerValue_CollectionChanged;
                OnValueChanged();
            }
        }

        public override object DefaultValue { get { return Value; } set { Value = value; } }

        public override void ResetValue()
        {
            foreach (var p in InnerValueAsDictionary.Values)
                p.ResetValue();
        }

        public override void BeginEdit()
        {
            base.BeginEdit();
            foreach (var p in InnerValueAsDictionary.Values)
                p.BeginEdit();
        }

        public override void SaveEdit()
        {
            base.SaveEdit();
            foreach (var p in InnerValueAsDictionary.Values)
                p.SaveEdit();
        }

        public override void DiscardEdit()
        {
            base.DiscardEdit();
            foreach (var p in InnerValueAsDictionary.Values)
                p.DiscardEdit();
        }

        private ObservableDictionary<string, Property2> InnerValueAsDictionary 
        { 
            get { return ((ObservableDictionary<string, Property2>)InnerValue); }
        }

        public override void ReadXml(XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name != "PropertyEntry") continue;
                try
                {
                    var kv = PropertyEntryMultiSerializer.ReadXml(reader);
                    InnerValueAsDictionary.Add(kv.Key, kv.Value);
                }
                catch
                {
                }
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            foreach (var key in InnerValueAsDictionary.Keys)
                PropertyEntryMultiSerializer.WriteXml(writer, key, InnerValueAsDictionary[key]);
        }
    }
}
