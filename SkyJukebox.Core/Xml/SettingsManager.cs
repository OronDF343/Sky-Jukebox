using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SkyJukebox.Lib.Collections;
using SkyJukebox.Lib.Xml;

namespace SkyJukebox.Core.Xml
{
    public class SettingsManager : ObservableDictionary<string, Property2>, IXmlSerializable
    {
        private SettingsManager()
        {
        }

        public static SettingsManager Instance { get; private set; }

        private static readonly AutoSerializer<SettingsManager> AutoSerializer = new AutoSerializer<SettingsManager>();
        public static string Path { get; private set; }
        public static void Init(string path)
        {
            Path = path;
            Instance = new SettingsManager
            {
                { "DisableAeroGlass", new ValueProperty2<bool>(false) },
                { "RestoreLocation", new ValueProperty2<bool>(false) },
                { "LastWindowLocation", new PointProperty2(new System.Windows.Point(0, 0)) },
                { "LoadPlaylistOnStartup", new ValueProperty2<bool>(false) },
                { "PlaylistToAutoLoad", new StringProperty2("placeholder.m3u") },
                { "ShowPlaylistEditorOnStartup", new ValueProperty2<bool>(false) },
                { "EnableRecolor", new ValueProperty2<bool>(false) },
                { "TextScrollingDelay", new ValueProperty2<double>(0.21) },
                { "GuiColor", new ColorProperty2(Color.Black) },
                { "ProgressColor", new ColorProperty2(Color.FromArgb(127, 31, 199, 15)) },
                { "BgColor", new ColorProperty2(Color.Transparent) },
                { "HeaderFormat", new StringProperty2("$PJ($AJ) - $TI($FN)") },
                { "PlaybackDevice", new GuidProperty2(Guid.Empty) },
                { "SelectedSkin", new StringProperty2("Default Skin") },
                {
                    "PlaylistEditorColumnsVisibility", new NestedProperty2(new ObservableDictionary<string, Property2>
                    {
                        { "FileName", new ValueProperty2<bool>(true) },
                        { "Title", new ValueProperty2<bool>(true) },
                        { "Performers", new ValueProperty2<bool>(false) },
                        { "AlbumArtists", new ValueProperty2<bool>(true) },
                        { "Album", new ValueProperty2<bool>(true) },
                        { "TrackNumber", new ValueProperty2<bool>(true) },
                        { "Genre", new ValueProperty2<bool>(false) },
                        { "Year", new ValueProperty2<bool>(false) },
                        { "Duration", new ValueProperty2<bool>(true) },
                        { "Codec", new ValueProperty2<bool>(false) },
                        { "Bitrate", new ValueProperty2<bool>(false) }
                    })
                }
            };
            Load();
        }

        public static void Load()
        {
            if (!File.Exists(Path)) return;
            var sm = AutoSerializer.LoadFromXml(Path);
            if (sm == null) return;
            foreach (KeyValuePair<string, Property2> p in sm)
            {
                if (Instance.ContainsKey(p.Key))
                    Instance[p.Key].Value = p.Value.Value;
                else
                    Instance.Add(p.Key, p.Value);
            }
        }

        public static void Save()
        {
            AutoSerializer.SaveToXml(Path, Instance);
        }

        #region IXmlSerializable Members
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
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

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var valueSerializer = new XmlSerializer(typeof(Property2));

            foreach (var key in Keys)
            {
                writer.WriteStartElement("PropertyEntry");

                writer.WriteAttributeString("Key", key);

                var value = this[key];
                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
