using SkyJukebox.CoreApi.Icons;
using SkyJukebox.CoreApi.Keyboard;
using SkyJukebox.CoreApi.Xml;
using SkyJukebox.CoreApi;
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
    public partial class App : Application, IDisposable
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
            // Error handling:
            AppDomain.CurrentDomain.UnhandledException +=
                (s, args) =>
                    MessageBox.Show(args.ExceptionObject.ToString(), "Fatal Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

            // Load skins:
            if (!Directory.Exists(InstanceManager.ExePath + InstanceManager.SkinsPath))
                Directory.CreateDirectory(InstanceManager.ExePath + InstanceManager.SkinsPath);
            else
                SkinManager.Instance.LoadAllSkins(InstanceManager.ExePath + InstanceManager.SkinsPath);

            // Load settings:
            Settings.Load(InstanceManager.ExePath + InstanceManager.SettingsPath);

            // Set skin:
            if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
            {
                MessageBox.Show("Failed to load skin: " + Settings.Instance.SelectedSkin, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Settings.Instance.SelectedSkin.ResetValue();
                if (!IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin))
                    MessageBox.Show("Failed to load fallback default skin!", "This is a bug!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

            // Load plugins:
            InstanceManager.LoadedPlugins = PluginInteraction.RegisterAllPlugins();

            // Get ClArgs:
            InstanceManager.CommmandLineArgs = Environment.GetCommandLineArgs();

            // Load key bindings:
            KeyBindingManager.Init(InstanceManager.ExePath + InstanceManager.KeyConfigPath);
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
            bool mutexCreated;
            var mutexName = ("SkyJukebox::{" + WindowsIdentity.GetCurrent().Name + "}").Replace('\\', '|');

            _mutex = new Mutex(true, mutexName, out mutexCreated);
            Message = (int)NativeMethods.RegisterWindowMessage(mutexName);

            if (!mutexCreated)
            {
                _mutex = null;
                ClArgs.WriteClArgsToFile(Environment.GetCommandLineArgs());
                NativeMethods.SendMessage(NativeMethods.HWND_BROADCAST, Message, IntPtr.Zero, IntPtr.Zero);
                //MessageBox.Show("Posted HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            MainWindow = InstanceManager.MiniPlayerInstance = new MiniPlayer();
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
