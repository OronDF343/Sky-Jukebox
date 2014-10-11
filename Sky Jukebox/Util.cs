using System.Reflection;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using NVorbis.NAudioSupport;
using SkyJukebox.Data;
using SkyJukebox.Display;
using SkyJukebox.Playback;
using SkyJukebox.PluginAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyJukebox
{
    public static class Util
    {
        public static void LoadStuff()
        {
            var epath = Assembly.GetExecutingAssembly().Location;
            var apath = epath.SubstringRange(0, epath.LastIndexOf('\\') + 1);
            Settings.Init(apath + Instance.SettingsPath);
            //if (Settings.Instance.LoadPlaylistOnStartup && File.Exists(Settings.Instance.PlaylistToAutoLoad))
            //    PlaybackManager.Instance = new BackgroundPlayer(new Playlist(Settings.Instance.PlaylistToAutoLoad));
            //else
            //    PlaybackManager.Instance = new BackgroundPlayer();
            // TODO: Fix playlist autoloading

            // load built-in codecs:
            NAudioPlayer.AddCodec(new string[] { "mp3", "wav", "m4a", "aac", "aiff", "mpc", "ape" }, typeof(AudioFileReader));
            NAudioPlayer.AddCodec(new string[] { "wma" }, typeof(WMAFileReader));
            NAudioPlayer.AddCodec(new string[] { "ogg" }, typeof(VorbisWaveReader));
            // moved to test codec:
            //BackgroundPlayer.AddCodec(new string[] { "flac" }, typeof(FlacFileReader));

            // load plugins:
            Instance.LoadedPlugins = PluginInteraction.GetPlugins(apath);
            foreach (ICodec c in PluginInteraction.GetCodecs(apath))
            {
                if (!c.WaveStreamType.IsSubclassOf(typeof(WaveStream)))
                    throw new InvalidOperationException("A plugin tried to register a codec which doesn't derive from WaveStream!");
                var e = from x in c.Extensions
                        select x.ToLower();
                NAudioPlayer.AddCodec(e, c.WaveStreamType);
            }

            // register the NAudioPlayer
            PlaybackManager.Instance.RegisterAudioPlayer(NAudioPlayer.GetCodecs(), new NAudioPlayer());
            foreach (IAudioPlayer a in PluginInteraction.GetAudioPlayers(apath))
            {
                var e = from x in a.Extensions
                        select x.ToLower();
                PlaybackManager.Instance.RegisterAudioPlayer(e, a);
            }
        }

        public static string SubstringRange(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string FormatHeader(Music m, string h)
        {
            var artists = string.Join(", ", m.TagFile.Tag.AlbumArtists);
            var title = m.TagFile.Tag.Title;
            return (artists == "" ? "Unknown Artist" : artists) + " - " + (title ?? m.FileName);
        }

        public static IEnumerable<string> GetFiles(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                foreach (var subDir in Directory.GetDirectories(path))
                    queue.Enqueue(subDir);
                var files = Directory.GetFiles(path);
                foreach (var t in files)
                {
                    yield return t;
                }
            }
        }

        public static void SavePlaylist(Playlist data, string file, bool relative)
        {
            data.ShuffleIndex = false;
            File.WriteAllLines(file, from m in data
                                     select relative ? MakeRelativePath(file, m.FilePath) : m.FilePath);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static double Round(double src)
        {
            return double.Parse(src.ToString("F2"));
        }

        public static void SetIconImage(this System.Windows.Controls.Image target, string key)
        {
            target.Source = Instance.IconImageDictionary[key].ToBitmapSource();
        }

        public static string GetExt(this string path)
        {
            return path.SubstringRange(path.LastIndexOf('.') + 1, path.Length);
        }
    }
}
