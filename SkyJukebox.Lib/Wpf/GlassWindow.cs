using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SkyJukebox.Lib.Wpf
{
    public class GlassWindow : Window
    {
        public GlassWindow()
        {
            Loaded += GlassWindow_Loaded;
            SourceInitialized += GlassWindow_SourceInitialized;
            MouseDown += GlassWindow_MouseDown;
            _windowInteropHelper = new WindowInteropHelper(this);
        }
        public bool DisableAeroGlass { get; set; }
        public bool AllowDrag { get; set; }
        private HwndSource _mainWindowSrc;
        private readonly WindowInteropHelper _windowInteropHelper;
        private void GlassWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DisableAeroGlass) return;
            Console.WriteLine("OS Version: " + Environment.OSVersion.Version.ToString());
            var handle = _windowInteropHelper.Handle;
            _mainWindowSrc = HwndSource.FromHwnd(handle);

            if (_mainWindowSrc == null || _mainWindowSrc.CompositionTarget == null)
            {
                MessageBox.Show("Error getting HwndSource! Window appearance can't be properly set!", "Exception in Loaded", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            _mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;
            var glassParams = new NativeMethods.DwmBlurbehind
            {
                dwFlags = NativeMethods.DwmBlurbehind.DWM_BB_ENABLE,
                fEnable = true,
                hRegionBlur = IntPtr.Zero
            };

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2)
            {
                var dis = 2;
                NativeMethods.DwmSetWindowAttribute(_mainWindowSrc.Handle,
                    NativeMethods.DwmWindowAttribute.DWMWA_LAST,
                    ref dis,
                    sizeof(uint));
            }
            else if (Environment.OSVersion.Version.Major > 6)
            {
                var m = new NativeMethods.MARGINS();
                m.cxLeftWidth = m.cxRightWidth = m.cyBottomHeight = m.cyTopHeight = -1;
                NativeMethods.DwmExtendFrameIntoClientArea(handle, ref m);
            }

            NativeMethods.DwmEnableBlurBehindWindow(handle, glassParams);
        }

        private void GlassWindow_SourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            if (source == null)
            {
                MessageBox.Show("Error getting HwndSource! Window appearance can't be properly set!", "Exception in SourceInitialized", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            source.AddHook(HwndSourceHook);
            var hWnd = _windowInteropHelper.Handle;
            var flags = NativeMethods.GetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/);
            var exclude = 0x00010000 | 0x00020000 | 0x00080000; // WS_MAXIMIZEBOX, WS_MINIMIZEBOX, WS_SYSMENU
#if WIN32
            var dwnl = flags & ~exclude;
#else
            var dwnl = new IntPtr(flags.ToInt64() & ~(long)exclude);
#endif
            NativeMethods.SetWindowLongPtr(hWnd, -16 /*GWL_STYLE*/, dwnl);
        }

        protected virtual IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0084) /*WM_NCHITTEST*/
            {
                var result = NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
                if (result.ToInt32() >= 10 /*HTLEFT*/ && result.ToInt32() <= 17 /*HTBOTTOMRIGHT*/ )
                {
                    handled = true;
                    return new IntPtr(18 /*HTBORDER*/);
                }
            }
            return IntPtr.Zero;
        }

        // Drag the window:
        private void GlassWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (AllowDrag) DragMove();
        }
    }
}
