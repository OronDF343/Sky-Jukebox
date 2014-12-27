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
    /// <summary>
    /// Provides access to all the settings of Sky Jukebox.
    /// </summary>
    public class SettingsManager : ObservableDictionary<string, Property2>, IXmlSerializable
    {
        private SettingsManager()
        {
        }

        /// <summary>
        /// Gets the current instance of the SettingsManager.
        /// </summary>
        public static SettingsManager Instance { get; private set; }

        private static readonly AutoSerializer<SettingsManager> AutoSerializer = new AutoSerializer<SettingsManager>();
        
        /// <summary>
        /// Gets the path of the settings file currently in use.
        /// </summary>
        public static string Path { get; private set; }
        
        /// <summary>
        /// Preforms initial loading of the global default settings, and then loads the settings from the file.
        /// </summary>
        /// <param name="path">The path to the settings file</param>
        public static void Init(string path)
        {
            Path = path;
            Instance = new SettingsManager
            {
                { "DisableAeroGlass", new BoolProperty2(false) },
                { "RestoreLocation", new BoolProperty2(false) },
                { "LastWindowLocation", new PointProperty2(new System.Windows.Point(0, 0)) },
                { "LoadPlaylistOnStartup", new BoolProperty2(false) },
                { "PlaylistToAutoLoad", new StringProperty2("placeholder.m3u") },
                { "ShowPlaylistEditorOnStartup", new BoolProperty2(false) },
                { "EnableRecolor", new BoolProperty2(false) },
                { "TextScrollingDelay", new DoubleProperty2(0.21) },
                { "GuiColor", new ColorProperty2(Color.Black) },
                { "ProgressColor", new ColorProperty2(Color.FromArgb(127, 31, 199, 15)) },
                { "BgColor", new ColorProperty2(Color.Transparent) },
                { "HeaderFormat", new StringProperty2("$PJ($AJ) - $TI($FN)") },
                { "PlaybackDevice", new GuidProperty2(Guid.Empty) },
                { "SelectedSkin", new StringProperty2("Default Skin") },
                {
                    "PlaylistEditorColumnsVisibility", new NestedProperty2(new ObservableDictionary<string, Property2>
                    {
                        { "FileName", new BoolProperty2(true) },
                        { "Title", new BoolProperty2(true) },
                        { "Performers", new BoolProperty2(false) },
                        { "AlbumArtists", new BoolProperty2(true) },
                        { "Album", new BoolProperty2(true) },
                        { "TrackNumber", new BoolProperty2(true) },
                        { "Genre", new BoolProperty2(false) },
                        { "Year", new BoolProperty2(false) },
                        { "Duration", new BoolProperty2(true) },
                        { "Codec", new BoolProperty2(false) },
                        { "Bitrate", new BoolProperty2(false) }
                    })
                }
            };
            Load();
        }

        /// <summary>
        /// Loads the settings from the file and overwrites them if they are already loaded.
        /// </summary>
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

        /// <summary>
        /// Saves the current settings to the file.
        /// </summary>
        public static void Save()
        {
            AutoSerializer.SaveToXml(Path, Instance);
        }

        #region Editing

        public bool IsGlobalEditInProgress { get; protected set; }

        public void ResetAll()
        {
            foreach (var p in Values)
                p.ResetValue();
        }

        public void BeginEditAll()
        {
            CheckAndSetIsGlobalEditInProgress(true);
            foreach (var p in Values)
                p.BeginEdit();
        }

        public void SaveEditAll()
        {
            CheckAndSetIsGlobalEditInProgress(false);
            foreach (var p in Values)
                p.SaveEdit();
        }

        public void DiscardEditAll()
        {
            CheckAndSetIsGlobalEditInProgress(false);
            foreach (var p in Values)
                p.DiscardEdit();
        }

        private void CheckAndSetIsGlobalEditInProgress(bool targetValue)
        {
            if (!IsGlobalEditInProgress && !targetValue)
                throw new InvalidOperationException("Not currently editing!");
            if (IsGlobalEditInProgress && targetValue)
                throw new InvalidOperationException("Already editing!");
            IsGlobalEditInProgress = targetValue;
        }

        #endregion

        // TODO: Custom getters?

        #region IXmlSerializable Members
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
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
                    Add(kv.Key, kv.Value);
                }
                catch
                {
                }
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in Keys)
                PropertyEntryMultiSerializer.WriteXml(writer, key, this[key]);
        }
        #endregion
    }
}
