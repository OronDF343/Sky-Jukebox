using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib.Extensions;
using SkyJukebox.Lib.Keyboard;
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

        private ExtensionAccess _extAccess;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Load skins:
            if (!DirectoryEx.Exists(InstanceManager.Instance.SkinsFolderPath)) DirectoryEx.CreateDirectory(InstanceManager.Instance.SkinsFolderPath);
            SkinManager.Instance.LoadAllSkins(InstanceManager.Instance.SkinsFolderPath);

            // Load settings:
            SettingsManager.Init(InstanceManager.Instance.SettingsFilePath);

            // Set skin:
            if (!IconManager.Instance.LoadFromSkin((string)SettingsManager.Instance["SelectedSkin"].Value))
            {
                MessageBox.Show("Failed to load skin: " + SettingsManager.Instance["SelectedSkin"].Value, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                SettingsManager.Instance["SelectedSkin"].ResetValue();
                if (!IconManager.Instance.LoadFromSkin((string)SettingsManager.Instance["SelectedSkin"].Value))
                    MessageBox.Show("Failed to load fallback default skin!", "This is a bug!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

            // Load key bindings:
            KeyBindingManager.Init(InstanceManager.Instance.KeyConfigFilePath);
            // 
            KeyBindingManager.Instance.Disable = !(bool)SettingsManager.Instance["EnableGlobalKeyBindings"].Value;
            SettingsManager.Instance["EnableGlobalKeyBindings"].PropertyChanged +=
                (s, args) => KeyBindingManager.Instance.Disable = !(bool)SettingsManager.Instance["EnableGlobalKeyBindings"].Value;

            // Load plugins:
            // VERY IMPORTANT: Force evaluation of IEnumerable
            InstanceManager.Instance.LoadedExtensions = ExtensionLoader.GetCompatibleExtensions<IExtension>(Lib.PathStringUtils.GetExePath()).ToList();
            _extAccess = new ExtensionAccess();
            foreach (var ex in InstanceManager.Instance.LoadedExtensions) ex.Instance.Init(_extAccess);
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
            InstanceManager.Instance.CommmandLineArgs = Environment.GetCommandLineArgs().ToList();

            bool mutexCreated;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            string mutexName = windowsIdentity != null ? ("SkyJukebox::{" + windowsIdentity.Name + "}").Replace('\\', '|') : "SkyJukebox::{NoUser}";
            _mutex = new Mutex(true, mutexName, out mutexCreated);
            Message = (int)NativeMethods.RegisterWindowMessage(mutexName);
            

            if (!mutexCreated)
            {
                if (!InstanceManager.Instance.CommmandLineArgs.Contains("--wait"))
                {
                    _mutex = null;
                    if (InstanceManager.Instance.CommmandLineArgs.Count > 1)
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
            MainWindow = InstanceManager.Instance.MiniPlayerInstance = new MiniPlayer();
            // Load PlaylistEditor
            InstanceManager.Instance.PlaylistEditorInstance = new PlaylistEditor();
            // Load SettingsWindow
            InstanceManager.Instance.SettingsWindowInstance = new SettingsWindow();

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
