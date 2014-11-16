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
            Settings.Load(InstanceManager.ExeDir + InstanceManager.SettingsPath);

            // Set skin:
            if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
            {
                MessageBox.Show("Failed to load skin: " + Settings.Instance.SelectedSkin, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Settings.Instance.SelectedSkin.ResetValue();
                if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
                    MessageBox.Show("Failed to load fallback default skin!", "This is a bug!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

            // Load plugins:
            InstanceManager.LoadedPlugins = PluginInteraction.RegisterAllExtensions();

            // Load key bindings:
            KeyBindingManager.Init(InstanceManager.ExeDir + InstanceManager.KeyConfigPath);
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Settings.SaveToXml();
            KeyBindingManager.SaveToXml();
        }
        #endregion

        #region Methods: Overrides
        protected override void OnStartup(StartupEventArgs e)
        {
            // Error handling:
            AppDomain.CurrentDomain.UnhandledException +=
                (s, args) =>
                    MessageBox.Show(args.ExceptionObject.ToString(), "Fatal Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

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
                    ClArgs.WriteClArgsToFile(Environment.GetCommandLineArgs());
                    NativeMethods.SendMessage(NativeMethods.HWND_BROADCAST, Message, IntPtr.Zero, IntPtr.Zero);
                    //MessageBox.Show("Posted HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
