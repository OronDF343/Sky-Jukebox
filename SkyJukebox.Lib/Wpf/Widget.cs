using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SkyJukebox.Lib.Wpf
{
    public class Widget : GlassWindow
    {
        public Widget()
        {
            ShowActivated = false;
            ShowInTaskbar = false;
            AllowDrag = false;
            HideTimeout = 1000;
            _closeThread = new Thread(WaitForClose) { IsBackground = true };
            MouseLeave += Widget_MouseLeave;
            MouseEnter += Widget_MouseEnter;
        }

        public enum WidgetRelativePosition
        {
            Above, Below, Left, Right
        }

        public enum WidgetAlignment
        {
            Left, Right, Top, Bottom, Center
        }

        /// <summary>
        /// Creates a new Widget shown near a <see cref="Control"/> in the specified parent <see cref="Window"/>, using the specified positioning, and the specified alignment.
        /// </summary>
        /// <param name="parentWindow">The parent <see cref="Window"/>.</param>
        /// <param name="showNear">The target <see cref="Control"/>. Must be a child of the parent <see cref="Window"/>.</param>
        /// <param name="relativePosition">The positioning of the Widget relative to the target <see cref="Control"/>.</param>
        /// <param name="alignment">The alignment of the Widget relative to the target <see cref="Control"/>. Must be horizontal for vertical positioning and vice versa.</param>
        /// <param name="allowOverlap">Determines whether the Widget will overlap the parent <see cref="Window"/>, or be positioned outside of it.</param>
        /// <param name="autoPosition">Determines whether the positioning will be automatically changed if there is no space. For example, if the positioning is Above and there is insufficient space above the window, it will be set to Below, but never Left or Right.</param>
        public void Initialize(Window parentWindow, Control showNear, WidgetRelativePosition relativePosition,
            WidgetAlignment alignment, bool allowOverlap, bool autoPosition)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            ParentWindow = parentWindow;
            ParentWindow.LocationChanged += (sender, e) => UpdatePosition();
            ShowNear = showNear;
            RelativePosition = relativePosition;
            Alignment = alignment;
            AllowOverlap = allowOverlap;
            AutoPosition = autoPosition;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var absolutePoint = ShowNear.PointToScreen(new Point(0, 0));
            var presentationSource = PresentationSource.FromVisual(ShowNear);
            if (presentationSource != null)
            {
                if (presentationSource.CompositionTarget != null)
                {
                    var transform = presentationSource.CompositionTarget.TransformFromDevice;
                    absolutePoint = transform.Transform(absolutePoint);
                }
            }
            if (AutoPosition)
            {
                switch (RelativePosition)
                {
                    case WidgetRelativePosition.Above:
                        if ((AllowOverlap ? absolutePoint.Y : ParentWindow.Top) < Height)
                            RelativePosition = WidgetRelativePosition.Below;
                        break;
                    case WidgetRelativePosition.Below:
                        if (SystemParameters.WorkArea.Bottom -
                            (AllowOverlap ? absolutePoint.Y + ShowNear.ActualHeight : ParentWindow.ActualHeight + ParentWindow.Top) <
                            Height)
                            RelativePosition = WidgetRelativePosition.Above;
                        break;
                    case WidgetRelativePosition.Left:
                        if ((AllowOverlap ? absolutePoint.X : ParentWindow.Left) < Width)
                            RelativePosition = WidgetRelativePosition.Right;
                        break;
                    case WidgetRelativePosition.Right:
                        if (SystemParameters.WorkArea.Right -
                            (AllowOverlap ? absolutePoint.X + ShowNear.ActualWidth : ParentWindow.ActualWidth + ParentWindow.Left) < Width)
                            RelativePosition = WidgetRelativePosition.Left;
                        break;
                }
            }
            switch (RelativePosition)
            {
                case WidgetRelativePosition.Above:
                    Top = AllowOverlap ? absolutePoint.Y - Height : ParentWindow.Top - Height;
                    break;
                case WidgetRelativePosition.Below:
                    Top = AllowOverlap ? absolutePoint.Y + ShowNear.ActualHeight : ParentWindow.Top + ParentWindow.ActualHeight;
                    break;
                case WidgetRelativePosition.Left:
                case WidgetRelativePosition.Right:
                    Top = absolutePoint.Y;
                    switch (Alignment)
                    {
                        case WidgetAlignment.Top:
                            break;
                        case WidgetAlignment.Bottom:
                            Top -= Height - ShowNear.ActualHeight;
                            break;
                        case WidgetAlignment.Center:
                            Top -= (Height - ShowNear.ActualHeight) / 2;
                            break;
                        default:
                            throw new InvalidOperationException("Alignment must be vertical for horizontal positioning!");
                    }
                    break;
            }
            switch (RelativePosition)
            {
                case WidgetRelativePosition.Left:
                    Left = AllowOverlap ? absolutePoint.X - Width : ParentWindow.Left - Width;
                    break;
                case WidgetRelativePosition.Right:
                    Left = AllowOverlap ? absolutePoint.X + ShowNear.ActualWidth : ParentWindow.Left + ParentWindow.ActualWidth;
                    break;
                case WidgetRelativePosition.Below:
                case WidgetRelativePosition.Above:
                    Left = absolutePoint.X;
                    switch (Alignment)
                    {
                        case WidgetAlignment.Left:
                            break;
                        case WidgetAlignment.Right:
                            Left -= Width - ShowNear.ActualWidth;
                            break;
                        case WidgetAlignment.Center:
                            Left -= (Width - ShowNear.ActualWidth) / 2;
                            break;
                        default:
                            throw new InvalidOperationException("Alignment must be horizontal for vertical positioning!");
                    }
                    break;
            }
        }

        public int HideTimeout { get; set; }
        protected Window ParentWindow { get; set; }
        protected Control ShowNear { get; set; }
        public WidgetRelativePosition RelativePosition { get; set; }
        public WidgetAlignment Alignment { get; set; }
        public bool AllowOverlap { get; set; }
        public bool AutoPosition { get; set; }

        private Thread _closeThread;

        private void WaitForClose()
        {
            Thread.Sleep(HideTimeout);
            Dispatcher.Invoke(Hide);
        }

        private void Widget_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_closeThread.IsAlive)
            {
                _closeThread = new Thread(WaitForClose) {IsBackground = true};
                _closeThread.Start();
            }
            ParentWindow.Focus();
        }

        private void Widget_MouseEnter(object sender, MouseEventArgs e)
        {
            _closeThread.Abort();
            Focus();
        }
    }
}
