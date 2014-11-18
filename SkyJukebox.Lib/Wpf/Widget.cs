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
            var absolutePoint = showNear.PointToScreen(new Point(0, 0));
            if (autoPosition)
            {
                switch (relativePosition)
                {
                    case WidgetRelativePosition.Above:
                        if ((allowOverlap ? absolutePoint.Y : ParentWindow.Top) < Height)
                            relativePosition = WidgetRelativePosition.Below;
                        break;
                    case WidgetRelativePosition.Below:
                        if (SystemParameters.WorkArea.Bottom -
                            (allowOverlap ? absolutePoint.Y + showNear.ActualHeight : ParentWindow.ActualHeight + ParentWindow.Top) <
                            Height)
                            relativePosition = WidgetRelativePosition.Above;
                        break;
                    case WidgetRelativePosition.Left:
                        if ((allowOverlap ? absolutePoint.X : ParentWindow.Left) < Width)
                            relativePosition = WidgetRelativePosition.Right;
                        break;
                    case WidgetRelativePosition.Right:
                        if (SystemParameters.WorkArea.Right -
                            (allowOverlap ? absolutePoint.X + showNear.ActualWidth : ParentWindow.ActualWidth + ParentWindow.Left) < Width)
                            relativePosition = WidgetRelativePosition.Left;
                        break;
                }
            }
            switch (relativePosition)
            {
                case WidgetRelativePosition.Above:
                    Top = allowOverlap ? absolutePoint.Y - Height : ParentWindow.Top - Height;
                    break;
                case WidgetRelativePosition.Below:
                    Top = allowOverlap ? absolutePoint.Y + showNear.ActualHeight : ParentWindow.Top + ParentWindow.ActualHeight;
                    break;
                case WidgetRelativePosition.Left:
                case WidgetRelativePosition.Right:
                    Top = absolutePoint.Y;
                    switch (alignment)
                    {
                        case WidgetAlignment.Top:
                            break;
                        case WidgetAlignment.Bottom:
                            Top -= Height - showNear.ActualHeight;
                            break;
                        case WidgetAlignment.Center:
                            Top -= (Height - showNear.ActualHeight) / 2;
                            break;
                        default:
                            throw new InvalidOperationException("Alignment must be vertical for horizontal positioning!");
                    }
                    break;
            }
            switch (relativePosition)
            {
                case WidgetRelativePosition.Left:
                    Left = allowOverlap ? absolutePoint.X - Width : ParentWindow.Left - Width;
                    break;
                case WidgetRelativePosition.Right:
                    Left = allowOverlap ? absolutePoint.X + showNear.ActualWidth : ParentWindow.Left + ParentWindow.ActualWidth;
                    break;
                case WidgetRelativePosition.Below:
                case WidgetRelativePosition.Above:
                    Left = absolutePoint.X;
                    switch (alignment)
                    {
                        case WidgetAlignment.Left:
                            break;
                        case WidgetAlignment.Right:
                            Left -= Width - showNear.ActualWidth;
                            break;
                        case WidgetAlignment.Center:
                            Left -= (Width - showNear.ActualWidth) / 2;
                            break;
                        default:
                            throw new InvalidOperationException("Alignment must be horizontal for vertical positioning!");
                    }
                    break;
            }
        }

        public int HideTimeout { get; set; }
        protected Window ParentWindow { get; set; }

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
