using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SkyJukebox.Lib.Xml
{
    public static class PropertyEntryMultiSerializer
    {
        private static readonly Dictionary<string, XmlSerializer> ValueSerializers = new Dictionary<string, XmlSerializer>();

        private static XmlSerializer GetSerializer(Type t)
        {
            if (t.AssemblyQualifiedName == null)
                throw new NullReferenceException("Failed to get the Assembly Qualified Name of the type " + t.Name);
            if (!ValueSerializers.ContainsKey(t.AssemblyQualifiedName))
                ValueSerializers.Add(t.AssemblyQualifiedName, new XmlSerializer(t));
            return ValueSerializers[t.AssemblyQualifiedName];
        }

        public static KeyValuePair<string, Property> ReadXml(XmlReader reader)
        {
            var key = reader.GetAttribute("Key");
            var type = reader.GetAttribute("Type");

            reader.MoveToElement();
            reader.ReadStartElement("PropertyEntry");
            Property value = null;
            try
            {
                var t = Type.GetType(type);
                value = (Property)GetSerializer(t).Deserialize(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deserialization failed: " + ex.Message);
                Console.WriteLine("Property Key: " + key);
                Console.WriteLine("Property Type Qualified Name: " + type);
                Console.WriteLine("Stacktrace: " + ex.StackTrace);
            }

            reader.ReadEndElement();
            reader.MoveToContent();
            if (value == null)
                throw new Exception();
            return new KeyValuePair<string, Property>(key, value);
        }

        public static void WriteXml(XmlWriter writer, string key, Property value)
        {
            writer.WriteStartElement("PropertyEntry");

            writer.WriteAttributeString("Key", key);

            var type = value.GetType();
            try
            {
                var ser = GetSerializer(type);
                writer.WriteAttributeString("Type", type.AssemblyQualifiedName);
                ser.Serialize(writer, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Serialization failed: " + ex.Message);
                Console.WriteLine("Property Key: " + key);
                Console.WriteLine("Property Value: " + value);
                Console.WriteLine("Property Type Name: " + type.Name);
                Console.WriteLine("Property Type Qualified Name: " + type.AssemblyQualifiedName);
                Console.WriteLine("Stacktrace: " + ex.StackTrace);
            }

            writer.WriteEndElement();
        }
    }
}
