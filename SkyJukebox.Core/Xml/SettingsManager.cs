using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SkyJukebox.Api;
using SkyJukebox.Lib.Collections;
using SkyJukebox.Lib.Xml;
using Point = System.Windows.Point;

namespace SkyJukebox.Core.Xml
{
    /// <summary>
    /// Provides access to all the settings of Sky Jukebox.
    /// </summary>
    public class SettingsManager : ObservableDictionary<string, Property>, ISettingsManager, IXmlSerializable
    {
        private SettingsManager() { }

        /// <summary>
        /// Gets the current instance of the SettingsManager.
        /// </summary>
        public static SettingsManager Instance { get; private set; }

        private static readonly AutoSerializer<SettingsManager> AutoSerializer = new AutoSerializer<SettingsManager>();
        
        /// <summary>
        /// Gets the path of the settings file currently in use.
        /// </summary>
        public string Path { get; private set; }
        
        /// <summary>
        /// Preforms initial loading of the global default settings, and then loads the settings from the file.
        /// </summary>
        /// <param name="path">The path to the settings file</param>
        public static void Init(string path)
        {
            Instance = new SettingsManager
            {
                { "DisableAeroGlass", new BoolProperty(false) },
                { "RestoreLocation", new BoolProperty(false) },
                { "LastWindowLocation", new PointProperty(new Point(0, 0)) },
                { "LoadPlaylistOnStartup", new BoolProperty(false) },
                { "PlaylistToAutoLoad", new StringProperty("placeholder.m3u") },
                { "ShowPlaylistEditorOnStartup", new BoolProperty(false) },
                { "EnableRecolor", new BoolProperty(false) },
                { "TextScrollingDelay", new DoubleProperty(0.21) },
                { "GuiColor", new ColorProperty(Color.Black) },
                { "ProgressColor", new ColorProperty(Color.FromArgb(127, 31, 199, 15)) },
                { "BgColor", new ColorProperty(Color.Transparent) },
                { "HeaderFormat", new StringProperty("$PJ($AJ) - $TI($FN)") },
                { "PlaybackDevice", new GuidProperty(Guid.Empty) },
                { "SelectedSkin", new StringProperty("Default Skin") },
                {
                    "PlaylistEditorColumnsVisibility", new NestedProperty(new ObservableDictionary<string, Property>
                    {
                        { "Number", new BoolProperty(false) },
                        { "FileName", new BoolProperty(true) },
                        { "Title", new BoolProperty(true) },
                        { "Performers", new BoolProperty(false) },
                        { "AlbumArtists", new BoolProperty(true) },
                        { "Album", new BoolProperty(true) },
                        { "TrackNumber", new BoolProperty(true) },
                        { "Genre", new BoolProperty(false) },
                        { "Year", new BoolProperty(false) },
                        { "Duration", new BoolProperty(true) },
                        { "Codec", new BoolProperty(false) },
                        { "Bitrate", new BoolProperty(false) }
                    })
                },
                { "EnableGlobalKeyBindings", new BoolProperty(false) },
                { "GlobalKeyBindingsOnlyWhenVisible", new BoolProperty(true) },
            };
            Instance.Path = path;
            Load();
        }

        /// <summary>
        /// Loads the settings from the file and overwrites them if they are already loaded.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(Instance.Path)) return;
            SettingsManager sm = null;
            try
            {
                sm = AutoSerializer.LoadFromXml(Instance.Path);
            }
            catch
            {
            }
            if (sm == null) return;
            foreach (KeyValuePair<string, Property> p in sm)
            {
                // TODO: Handle nested properties
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
            if (Instance != null)
                AutoSerializer.SaveToXml(Instance.Path, Instance);
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
