using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

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
        #endregion

        #region Methods: Overrides
        protected override void OnStartup(StartupEventArgs e)
        {
            bool mutexCreated;
            string mutexName = ("SkyJukebox::{" + WindowsIdentity.GetCurrent().Name + "}").Replace('\\', '|');

            _mutex = new Mutex(true, mutexName, out mutexCreated);
            Message = (int)NativeMethods.RegisterWindowMessage(mutexName);

            if (!mutexCreated)
            {
                _mutex = null;
                Util.WriteClArgsToFile(Environment.GetCommandLineArgs());
                NativeMethods.SendMessage(NativeMethods.HWND_BROADCAST, Message, IntPtr.Zero, IntPtr.Zero);
                //MessageBox.Show("Posted HWND message", "Debug", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            MainWindow = Instance.MiniPlayerInstance = new MiniPlayer();
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
