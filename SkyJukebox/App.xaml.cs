using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Keyboard;
using SkyJukebox.Core.Xml;
using SkyJukebox.Core;
using SkyJukebox.Utils;
using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
    {
        #region Members
        internal int Message;
        private Mutex _mutex;
        #endregion

        #region Methods: Functions
        private void Dispose(Boolean disposing)
        {
            if (disposing && (_mutex != null))
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
                _mutex = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Load skins:
            if (!Directory.Exists(InstanceManager.ExeDir + InstanceManager.SkinsPath))
                Directory.CreateDirectory(InstanceManager.ExeDir + InstanceManager.SkinsPath);
            else
                SkinManager.Instance.LoadAllSkins(InstanceManager.ExeDir + InstanceManager.SkinsPath);

            // Load settings:
            SettingsManager.Init(InstanceManager.ExeDir + InstanceManager.SettingsPath);

            // Set skin:
            if (!IconManager.Instance.LoadFromSkin((string)SettingsManager.Instance["SelectedSkin"].Value))
            {
                MessageBox.Show("Failed to load skin: " + SettingsManager.Instance["SelectedSkin"].Value, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                SettingsManager.Instance["SelectedSkin"].ResetValue();
                if (!IconManager.Instance.LoadFromSkin((string)SettingsManager.Instance["SelectedSkin"].Value))
                    MessageBox.Show("Failed to load fallback default skin!", "This is a bug!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

            // Load plugins:
            InstanceManager.LoadedPlugins = PluginInteraction.RegisterAllExtensions();
            // Temp code:
            var pa = new PluginAccess();
            foreach (var p in InstanceManager.LoadedPlugins)
                IconManager.Instance.Add(p.Attribute.Id, p.Instance.Load(pa));

            // Load key bindings:
            KeyBindingManager.Init(InstanceManager.ExeDir + InstanceManager.KeyConfigPath);
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
                _mutex = null;
            }
            SettingsManager.Save();
            KeyBindingManager.SaveToXml();
            if (InstanceManager.LoadedPlugins == null) return;
            foreach (var p in InstanceManager.LoadedPlugins)
                p.Instance.Unload();
        }
        #endregion

        #region Methods: Overrides
        protected override void OnStartup(StartupEventArgs e)
        {
            // Error handling:
            AppDomain.CurrentDomain.UnhandledException +=
                (s, args) =>
                {
                    var em = new ErrorMessage((Exception)args.ExceptionObject);
                    em.ShowDialog();
                    
                };

            // Set Priority: 
            // TODO: Setting for this
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            // Get ClArgs:
            InstanceManager.CommmandLineArgs = Environment.GetCommandLineArgs().ToList();

            bool mutexCreated;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            string mutexName = windowsIdentity != null ? ("SkyJukebox::{" + windowsIdentity.Name + "}").Replace('\\', '|') : "SkyJukebox::{NoUser}";
            _mutex = new Mutex(true, mutexName, out mutexCreated);
            Message = (int)NativeMethods.RegisterWindowMessage(mutexName);
            

            if (!mutexCreated)
            {
                if (!InstanceManager.CommmandLineArgs.Contains("--wait"))
                {
                    _mutex = null;
                    if (InstanceManager.CommmandLineArgs.Count > 1)
                    {
                        ClArgs.WriteClArgsToFile(Environment.GetCommandLineArgs());
                        NativeMethods.SendMessage(NativeMethods.HWND_BROADCAST, Message, IntPtr.Zero, IntPtr.Zero);
                        //MessageBox.Show("Posted HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    Current.Shutdown();
                    return;
                }
                if (!_mutex.WaitOne(20000))
                {
                    MessageBox.Show("Operation has timed out.", "Waiting for previous thread to exit",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }
                if (!Mutex.TryOpenExisting(mutexName, MutexRights.FullControl, out _mutex))
                {
                    _mutex = new Mutex(true, mutexName, out mutexCreated);
                    if (!mutexCreated)
                    {
                        MessageBox.Show("Previous thread exited, but this thread failed to gain access to the mutex or create a new one!", "Waiting for previous thread to exit",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                        Current.Shutdown();
                        return;
                    }
                }
            }

            base.OnStartup(e);

            // Order is important!
            // Load MiniPlayer
            MainWindow = InstanceManager.MiniPlayerInstance = new MiniPlayer();
            // Load PlaylistEditor
            InstanceManager.PlaylistEditorInstance = new PlaylistEditor();
            // Load SettingsWindow
            InstanceManager.SettingsWindowInstance = new SettingsWindow();

            // Continue
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dispose();
            base.OnExit(e);
        }
        #endregion
    }
}
