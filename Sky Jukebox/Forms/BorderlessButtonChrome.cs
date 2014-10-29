using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SkyJukebox.Forms
{
    // Credit to Fredrik Hedblad on Stack Overflow for the idea
    public sealed class BorderlessButtonChrome : Decorator
    {
        private static Pen _commonBorderPen;
        private static Pen _commonDefaultedInnerBorderPen;
        private static SolidColorBrush _commonDisabledBackgroundOverlay;
        private static Pen _commonDisabledBorderOverlay;
        private static LinearGradientBrush _commonHoverBackgroundOverlay;
        private static Pen _commonHoverBorderOverlay;
        private static Pen _commonInnerBorderPen;
        private static LinearGradientBrush _commonPressedBackgroundOverlay;
        private static Pen _commonPressedBorderOverlay;
        private static LinearGradientBrush _commonPressedLeftDropShadowBrush;
        private static LinearGradientBrush _commonPressedTopDropShadowBrush;
        private LocalResources _localResources;
        private static object _resourceAccess = new object();
        public static readonly DependencyProperty BackgroundProperty = Control.BackgroundProperty.AddOwner(typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BorderBrushProperty = Border.BorderBrushProperty.AddOwner(typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RenderDefaultedProperty = DependencyProperty.Register("RenderDefaulted", typeof(bool), typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRenderDefaultedChanged)));
        public static readonly DependencyProperty RenderMouseOverProperty = DependencyProperty.Register("RenderMouseOver", typeof(bool), typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRenderMouseOverChanged)));
        public static readonly DependencyProperty RenderPressedProperty = DependencyProperty.Register("RenderPressed", typeof(bool), typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRenderPressedChanged)));
        public static readonly DependencyProperty RoundCornersProperty = DependencyProperty.Register("RoundCorners", typeof(bool), typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty DisableInnerBorderProperty = DependencyProperty.Register("DisableInnerBorder", typeof(bool), typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        static BorderlessButtonChrome()
        {
            IsEnabledProperty.OverrideMetadata(typeof(BorderlessButtonChrome), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect rect;
            rect = new Rect
            {
                Width = Math.Max(0.0, finalSize.Width - 4.0),
                Height = Math.Max(0.0, finalSize.Height - 4.0),
            };
            rect.X = (finalSize.Width - rect.Width) * 0.5;
            rect.Y = (finalSize.Height - rect.Height) * 0.5;
            UIElement child = Child;
            if (child != null)
            {
                child.Arrange(rect);
            }
            return finalSize;
        }

        private void DrawBackground(DrawingContext dc, ref Rect bounds)
        {
            if (base.IsEnabled || RoundCorners)
            {
                Brush background = Background;
                if ((bounds.Width > 4.0) && (bounds.Height > 4.0))
                {
                    Rect rectangle = new Rect(bounds.Left + 1.0, bounds.Top + 1.0, bounds.Width - 2.0, bounds.Height - 2.0);
                    if (background != null)
                    {
                        dc.DrawRectangle(background, null, rectangle);
                    }
                    background = BackgroundOverlay;
                    if (background != null)
                    {
                        dc.DrawRectangle(background, null, rectangle);
                    }
                }
            }
        }

        private void DrawBorder(DrawingContext dc, ref Rect bounds)
        {
            if ((bounds.Width >= 5.0) && (bounds.Height >= 5.0))
            {
                Brush borderBrush = BorderBrush;
                Pen pen = null;
                if (borderBrush != null)
                {
                    if (_commonBorderPen == null)
                    {
                        lock (_resourceAccess)
                        {
                            if (_commonBorderPen == null)
                            {
                                if (!borderBrush.IsFrozen && borderBrush.CanFreeze)
                                {
                                    borderBrush = borderBrush.Clone();
                                    borderBrush.Freeze();
                                }
                                Pen pen2 = new Pen(borderBrush, 1.0);
                                if (pen2.CanFreeze)
                                {
                                    pen2.Freeze();
                                    _commonBorderPen = pen2;
                                }
                            }
                        }
                    }
                    if ((_commonBorderPen != null) && (borderBrush == _commonBorderPen.Brush))
                    {
                        pen = _commonBorderPen;
                    }
                    else
                    {
                        if (!borderBrush.IsFrozen && borderBrush.CanFreeze)
                        {
                            borderBrush = borderBrush.Clone();
                            borderBrush.Freeze();
                        }
                        pen = new Pen(borderBrush, 1.0);
                        if (pen.CanFreeze)
                        {
                            pen.Freeze();
                        }
                    }
                }
                Pen borderOverlayPen = BorderOverlayPen;
                if ((pen != null) || (borderOverlayPen != null))
                {
                    if (RoundCorners)
                    {
                        Rect rectangle = new Rect(bounds.Left + 0.5, bounds.Top + 0.5, bounds.Width - 1.0, bounds.Height - 1.0);
                        if (base.IsEnabled && (pen != null))
                        {
                            dc.DrawRoundedRectangle(null, pen, rectangle, 2.75, 2.75);
                        }
                        if (borderOverlayPen != null)
                        {
                            dc.DrawRoundedRectangle(null, borderOverlayPen, rectangle, 2.75, 2.75);
                        }
                    }
                    else
                    {
                        PathFigure figure = new PathFigure
                        {
                            StartPoint = new Point(0.5, 0.5)
                        };
                        figure.Segments.Add(new LineSegment(new Point(0.5, bounds.Bottom - 0.5), true));
                        figure.Segments.Add(new LineSegment(new Point(bounds.Right - 2.5, bounds.Bottom - 0.5), true));
                        figure.Segments.Add(new ArcSegment(new Point(bounds.Right - 0.5, bounds.Bottom - 2.5), new Size(2.0, 2.0), 0.0, false, SweepDirection.Counterclockwise, true));
                        figure.Segments.Add(new LineSegment(new Point(bounds.Right - 0.5, bounds.Top + 2.5), true));
                        figure.Segments.Add(new ArcSegment(new Point(bounds.Right - 2.5, bounds.Top + 0.5), new Size(2.0, 2.0), 0.0, false, SweepDirection.Counterclockwise, true));
                        figure.IsClosed = true;
                        PathGeometry geometry = new PathGeometry
                        {
                            Figures = { figure }
                        };
                        if (base.IsEnabled && (pen != null))
                        {
                            dc.DrawGeometry(null, pen, geometry);
                        }
                        if (borderOverlayPen != null)
                        {
                            dc.DrawGeometry(null, borderOverlayPen, geometry);
                        }
                    }
                }
            }
        }
        internal int EffectiveValuesInitialSize
        {
            get
            {
                return 9;
            }
        }

        private void DrawDropShadows(DrawingContext dc, ref Rect bounds)
        {
            if ((bounds.Width > 4.0) && (bounds.Height > 4.0))
            {
                Brush leftDropShadowBrush = LeftDropShadowBrush;
                if (leftDropShadowBrush != null)
                {
                    dc.DrawRectangle(leftDropShadowBrush, null, new Rect(1.0, 1.0, 2.0, bounds.Bottom - 2.0));
                }
                Brush topDropShadowBrush = TopDropShadowBrush;
                if (topDropShadowBrush != null)
                {
                    dc.DrawRectangle(topDropShadowBrush, null, new Rect(1.0, 1.0, bounds.Right - 2.0, 2.0));
                }
            }
        }

        private void DrawInnerBorder(DrawingContext dc, ref Rect bounds)
        {
            if (DisableInnerBorder == false && ((base.IsEnabled || RoundCorners) && ((bounds.Width >= 4.0) && (bounds.Height >= 4.0))))
            {
                Pen innerBorderPen = InnerBorderPen;
                if (innerBorderPen != null)
                {
                    dc.DrawRoundedRectangle(null, innerBorderPen, new Rect(bounds.Left + 1.5, bounds.Top + 1.5, bounds.Width - 3.0, bounds.Height - 3.0), 1.75, 1.75);
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElement child = Child;
            if (child != null)
            {
                Size size2 = new Size();
                bool flag = availableSize.Width < 4.0;
                bool flag2 = availableSize.Height < 4.0;
                if (!flag)
                {
                    size2.Width = availableSize.Width - 4.0;
                }
                if (!flag2)
                {
                    size2.Height = availableSize.Height - 4.0;
                }
                child.Measure(size2);
                Size desiredSize = child.DesiredSize;
                if (!flag)
                {
                    desiredSize.Width += 4.0;
                }
                if (!flag2)
                {
                    desiredSize.Height += 4.0;
                }
                return desiredSize;
            }
            return new Size(Math.Min(4.0, availableSize.Width), Math.Min(4.0, availableSize.Height));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect bounds = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight);
            DrawBackground(drawingContext, ref bounds);
            DrawDropShadows(drawingContext, ref bounds);
            DrawBorder(drawingContext, ref bounds);
            DrawInnerBorder(drawingContext, ref bounds);
        }

        private static void OnRenderDefaultedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BorderlessButtonChrome chrome = (BorderlessButtonChrome)o;
            if (chrome.Animates)
            {
                if (!chrome.RenderPressed)
                {
                    if ((bool)e.NewValue)
                    {
                        if (chrome._localResources == null)
                        {
                            chrome._localResources = new LocalResources();
                            chrome.InvalidateVisual();
                        }
                        Duration duration = new Duration(TimeSpan.FromSeconds(0.3));
                        ColorAnimation animation = new ColorAnimation(Color.FromArgb(0xf9, 0, 0xcc, 0xff), duration);
                        GradientStopCollection gradientStops = ((LinearGradientBrush)chrome.InnerBorderPen.Brush).GradientStops;
                        gradientStops[0].BeginAnimation(GradientStop.ColorProperty, animation);
                        gradientStops[1].BeginAnimation(GradientStop.ColorProperty, animation);
                        DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
                        timeline.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, TimeSpan.FromSeconds(0.5)));
                        timeline.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.0, TimeSpan.FromSeconds(0.75)));
                        timeline.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, TimeSpan.FromSeconds(2.0)));
                        timeline.RepeatBehavior = RepeatBehavior.Forever;
                        Timeline.SetDesiredFrameRate(timeline, 10);
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, timeline);
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, timeline);
                    }
                    else if (chrome._localResources == null)
                    {
                        chrome.InvalidateVisual();
                    }
                    else
                    {
                        Duration duration2 = new Duration(TimeSpan.FromSeconds(0.2));
                        DoubleAnimation animation2 = new DoubleAnimation
                        {
                            Duration = duration2
                        };
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, animation2);
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation2);
                        ColorAnimation animation3 = new ColorAnimation
                        {
                            Duration = duration2
                        };
                        GradientStopCollection stops2 = ((LinearGradientBrush)chrome.InnerBorderPen.Brush).GradientStops;
                        stops2[0].BeginAnimation(GradientStop.ColorProperty, animation3);
                        stops2[1].BeginAnimation(GradientStop.ColorProperty, animation3);
                    }
                }
            }
            else
            {
                chrome._localResources = null;
                chrome.InvalidateVisual();
            }
        }

        private static void OnRenderMouseOverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BorderlessButtonChrome chrome = (BorderlessButtonChrome)o;
            if (chrome.Animates)
            {
                if (!chrome.RenderPressed)
                {
                    if ((bool)e.NewValue)
                    {
                        if (chrome._localResources == null)
                        {
                            chrome._localResources = new LocalResources();
                            chrome.InvalidateVisual();
                        }
                        Duration duration = new Duration(TimeSpan.FromSeconds(0.3));
                        DoubleAnimation animation = new DoubleAnimation(1.0, duration);
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, animation);
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation);
                    }
                    else if (chrome._localResources == null)
                    {
                        chrome.InvalidateVisual();
                    }
                    else if (chrome.RenderDefaulted)
                    {
                        double opacity = chrome.BackgroundOverlay.Opacity;
                        double num2 = (1.0 - opacity) * 0.5;
                        DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
                        timeline.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, TimeSpan.FromSeconds(num2)));
                        timeline.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.0, TimeSpan.FromSeconds(num2 + 0.25)));
                        timeline.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, TimeSpan.FromSeconds(num2 + 1.5)));
                        timeline.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, TimeSpan.FromSeconds(2.0)));
                        timeline.RepeatBehavior = RepeatBehavior.Forever;
                        Timeline.SetDesiredFrameRate(timeline, 10);
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, timeline);
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, timeline);
                    }
                    else
                    {
                        Duration duration2 = new Duration(TimeSpan.FromSeconds(0.2));
                        DoubleAnimation animation2 = new DoubleAnimation
                        {
                            Duration = duration2
                        };
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation2);
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, animation2);
                    }
                }
            }
            else
            {
                chrome._localResources = null;
                chrome.InvalidateVisual();
            }
        }

        private static void OnRenderPressedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BorderlessButtonChrome chrome = (BorderlessButtonChrome)o;
            if (chrome.Animates)
            {
                if ((bool)e.NewValue)
                {
                    if (chrome._localResources == null)
                    {
                        chrome._localResources = new LocalResources();
                        chrome.InvalidateVisual();
                    }
                    Duration duration = new Duration(TimeSpan.FromSeconds(0.1));
                    DoubleAnimation animation = new DoubleAnimation(1.0, duration);
                    chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation);
                    chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, animation);
                    chrome.LeftDropShadowBrush.BeginAnimation(Brush.OpacityProperty, animation);
                    chrome.TopDropShadowBrush.BeginAnimation(Brush.OpacityProperty, animation);
                    animation = new DoubleAnimation(0.0, duration);
                    chrome.InnerBorderPen.Brush.BeginAnimation(Brush.OpacityProperty, animation);
                    ColorAnimation animation2 = new ColorAnimation(Color.FromRgb(0xc2, 0xe4, 0xf6), duration);
                    GradientStopCollection gradientStops = ((LinearGradientBrush)chrome.BackgroundOverlay).GradientStops;
                    gradientStops[0].BeginAnimation(GradientStop.ColorProperty, animation2);
                    gradientStops[1].BeginAnimation(GradientStop.ColorProperty, animation2);
                    animation2 = new ColorAnimation(Color.FromRgb(0xab, 0xda, 0xf3), duration);
                    gradientStops[2].BeginAnimation(GradientStop.ColorProperty, animation2);
                    animation2 = new ColorAnimation(Color.FromRgb(0x90, 0xcb, 0xeb), duration);
                    gradientStops[3].BeginAnimation(GradientStop.ColorProperty, animation2);
                    animation2 = new ColorAnimation(Color.FromRgb(0x2c, 0x62, 0x8b), duration);
                    chrome.BorderOverlayPen.Brush.BeginAnimation(SolidColorBrush.ColorProperty, animation2);
                }
                else if (chrome._localResources == null)
                {
                    chrome.InvalidateVisual();
                }
                else
                {
                    bool renderMouseOver = chrome.RenderMouseOver;
                    Duration duration2 = new Duration(TimeSpan.FromSeconds(0.1));
                    DoubleAnimation animation3 = new DoubleAnimation
                    {
                        Duration = duration2
                    };
                    chrome.LeftDropShadowBrush.BeginAnimation(Brush.OpacityProperty, animation3);
                    chrome.TopDropShadowBrush.BeginAnimation(Brush.OpacityProperty, animation3);
                    chrome.InnerBorderPen.Brush.BeginAnimation(Brush.OpacityProperty, animation3);
                    if (!renderMouseOver)
                    {
                        chrome.BorderOverlayPen.Brush.BeginAnimation(Brush.OpacityProperty, animation3);
                        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation3);
                    }
                    ColorAnimation animation4 = new ColorAnimation
                    {
                        Duration = duration2
                    };
                    chrome.BorderOverlayPen.Brush.BeginAnimation(SolidColorBrush.ColorProperty, animation4);
                    GradientStopCollection stops2 = ((LinearGradientBrush)chrome.BackgroundOverlay).GradientStops;
                    stops2[0].BeginAnimation(GradientStop.ColorProperty, animation4);
                    stops2[1].BeginAnimation(GradientStop.ColorProperty, animation4);
                    stops2[2].BeginAnimation(GradientStop.ColorProperty, animation4);
                    stops2[3].BeginAnimation(GradientStop.ColorProperty, animation4);
                }
            }
            else
            {
                chrome._localResources = null;
                chrome.InvalidateVisual();
            }
        }

        private bool Animates
        {
            get
            {
                return ((((SystemParameters.PowerLineStatus == PowerLineStatus.Online) && SystemParameters.ClientAreaAnimation) && (RenderCapability.Tier > 0)) && base.IsEnabled);
            }
        }

        public Brush Background
        {
            get
            {
                return (Brush)base.GetValue(BackgroundProperty);
            }
            set
            {
                base.SetValue(BackgroundProperty, value);
            }
        }

        private Brush BackgroundOverlay
        {
            get
            {
                if (!base.IsEnabled)
                {
                    return CommonDisabledBackgroundOverlay;
                }
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return CommonPressedBackgroundOverlay;
                    }
                    if (RenderMouseOver)
                    {
                        return CommonHoverBackgroundOverlay;
                    }
                    return null;
                }
                if (_localResources == null)
                {
                    return null;
                }
                if (_localResources.BackgroundOverlay == null)
                {
                    _localResources.BackgroundOverlay = CommonHoverBackgroundOverlay.Clone();
                    _localResources.BackgroundOverlay.Opacity = 0.0;
                }
                return _localResources.BackgroundOverlay;
            }
        }

        public Brush BorderBrush
        {
            get
            {
                return (Brush)base.GetValue(BorderBrushProperty);
            }
            set
            {
                base.SetValue(BorderBrushProperty, value);
            }
        }

        private Pen BorderOverlayPen
        {
            get
            {
                if (!base.IsEnabled)
                {
                    if (RoundCorners)
                    {
                        return CommonDisabledBorderOverlay;
                    }
                    return null;
                }
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return CommonPressedBorderOverlay;
                    }
                    if (RenderMouseOver)
                    {
                        return CommonHoverBorderOverlay;
                    }
                    return null;
                }
                if (_localResources == null)
                {
                    return null;
                }
                if (_localResources.BorderOverlayPen == null)
                {
                    _localResources.BorderOverlayPen = CommonHoverBorderOverlay.Clone();
                    _localResources.BorderOverlayPen.Brush.Opacity = 0.0;
                }
                return _localResources.BorderOverlayPen;
            }
        }

        private static Pen CommonDefaultedInnerBorderPen
        {
            get
            {
                if (_commonDefaultedInnerBorderPen == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonDefaultedInnerBorderPen == null)
                        {
                            Pen pen = new Pen
                            {
                                Thickness = 1.0,
                                Brush = new SolidColorBrush(Color.FromArgb(0xf9, 0, 0xcc, 0xff))
                            };
                            pen.Freeze();
                            _commonDefaultedInnerBorderPen = pen;
                        }
                    }
                }
                return _commonDefaultedInnerBorderPen;
            }
        }

        private static SolidColorBrush CommonDisabledBackgroundOverlay
        {
            get
            {
                if (_commonDisabledBackgroundOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonDisabledBackgroundOverlay == null)
                        {
                            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(0xf4, 0xf4, 0xf4));
                            brush.Freeze();
                            _commonDisabledBackgroundOverlay = brush;
                        }
                    }
                }
                return _commonDisabledBackgroundOverlay;
            }
        }

        private static Pen CommonDisabledBorderOverlay
        {
            get
            {
                if (_commonDisabledBorderOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonDisabledBorderOverlay == null)
                        {
                            Pen pen = new Pen
                            {
                                Thickness = 1.0,
                                Brush = new SolidColorBrush(Color.FromRgb(0xad, 0xb2, 0xb5))
                            };
                            pen.Freeze();
                            _commonDisabledBorderOverlay = pen;
                        }
                    }
                }
                return _commonDisabledBorderOverlay;
            }
        }

        private static LinearGradientBrush CommonHoverBackgroundOverlay
        {
            get
            {
                if (_commonHoverBackgroundOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonHoverBackgroundOverlay == null)
                        {
                            LinearGradientBrush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.0, 0.0),
                                EndPoint = new Point(0.0, 1.0),
                                GradientStops = { new GradientStop(Color.FromArgb(0xff, 0xea, 0xf6, 0xfd), 0.0), new GradientStop(Color.FromArgb(0xff, 0xd9, 240, 0xfc), 0.5), new GradientStop(Color.FromArgb(0xff, 190, 230, 0xfd), 0.5), new GradientStop(Color.FromArgb(0xff, 0xa7, 0xd9, 0xf5), 1.0) }
                            };
                            brush.Freeze();
                            _commonHoverBackgroundOverlay = brush;
                        }
                    }
                }
                return _commonHoverBackgroundOverlay;
            }
        }

        private static Pen CommonHoverBorderOverlay
        {
            get
            {
                if (_commonHoverBorderOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonHoverBorderOverlay == null)
                        {
                            Pen pen = new Pen
                            {
                                Thickness = 1.0,
                                Brush = new SolidColorBrush(Color.FromRgb(60, 0x7f, 0xb1))
                            };
                            pen.Freeze();
                            _commonHoverBorderOverlay = pen;
                        }
                    }
                }
                return _commonHoverBorderOverlay;
            }
        }

        private static Pen CommonInnerBorderPen
        {
            get
            {
                if (_commonInnerBorderPen == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonInnerBorderPen == null)
                        {
                            Pen pen = new Pen
                            {
                                Thickness = 1.0
                            };
                            LinearGradientBrush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.0, 0.0),
                                EndPoint = new Point(0.0, 1.0),
                                GradientStops = { new GradientStop(Color.FromArgb(250, 0xff, 0xff, 0xff), 0.0), new GradientStop(Color.FromArgb(0x85, 0xff, 0xff, 0xff), 1.0) }
                            };
                            pen.Brush = brush;
                            pen.Freeze();
                            _commonInnerBorderPen = pen;
                        }
                    }
                }
                return _commonInnerBorderPen;
            }
        }

        private static LinearGradientBrush CommonPressedBackgroundOverlay
        {
            get
            {
                if (_commonPressedBackgroundOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonPressedBackgroundOverlay == null)
                        {
                            LinearGradientBrush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.0, 0.0),
                                EndPoint = new Point(0.0, 1.0),
                                GradientStops = { new GradientStop(Color.FromArgb(0xff, 0xc2, 0xe4, 0xf6), 0.5), new GradientStop(Color.FromArgb(0xff, 0xab, 0xda, 0xf3), 0.5), new GradientStop(Color.FromArgb(0xff, 0x90, 0xcb, 0xeb), 1.0) }
                            };
                            brush.Freeze();
                            _commonPressedBackgroundOverlay = brush;
                        }
                    }
                }
                return _commonPressedBackgroundOverlay;
            }
        }

        private static Pen CommonPressedBorderOverlay
        {
            get
            {
                if (_commonPressedBorderOverlay == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonPressedBorderOverlay == null)
                        {
                            Pen pen = new Pen
                            {
                                Thickness = 1.0,
                                Brush = new SolidColorBrush(Color.FromRgb(0x2c, 0x62, 0x8b))
                            };
                            pen.Freeze();
                            _commonPressedBorderOverlay = pen;
                        }
                    }
                }
                return _commonPressedBorderOverlay;
            }
        }

        private static LinearGradientBrush CommonPressedLeftDropShadowBrush
        {
            get
            {
                if (_commonPressedLeftDropShadowBrush == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonPressedLeftDropShadowBrush == null)
                        {
                            LinearGradientBrush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.0, 0.0),
                                EndPoint = new Point(1.0, 0.0),
                                GradientStops = { new GradientStop(Color.FromArgb(0x80, 0x33, 0x33, 0x33), 0.0), new GradientStop(Color.FromArgb(0, 0x33, 0x33, 0x33), 1.0) }
                            };
                            brush.Freeze();
                            _commonPressedLeftDropShadowBrush = brush;
                        }
                    }
                }
                return _commonPressedLeftDropShadowBrush;
            }
        }

        private static LinearGradientBrush CommonPressedTopDropShadowBrush
        {
            get
            {
                if (_commonPressedTopDropShadowBrush == null)
                {
                    lock (_resourceAccess)
                    {
                        if (_commonPressedTopDropShadowBrush == null)
                        {
                            LinearGradientBrush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.0, 0.0),
                                EndPoint = new Point(0.0, 1.0),
                                GradientStops = { new GradientStop(Color.FromArgb(0x80, 0x33, 0x33, 0x33), 0.0), new GradientStop(Color.FromArgb(0, 0x33, 0x33, 0x33), 1.0) }
                            };
                            brush.Freeze();
                            _commonPressedTopDropShadowBrush = brush;
                        }
                    }
                }
                return _commonPressedTopDropShadowBrush;
            }
        }

        private Pen InnerBorderPen
        {
            get
            {
                if (!base.IsEnabled)
                {
                    return CommonInnerBorderPen;
                }
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return null;
                    }
                    if (RenderDefaulted)
                    {
                        return CommonDefaultedInnerBorderPen;
                    }
                    return CommonInnerBorderPen;
                }
                if (_localResources == null)
                {
                    return CommonInnerBorderPen;
                }
                if (_localResources.InnerBorderPen == null)
                {
                    _localResources.InnerBorderPen = CommonInnerBorderPen.Clone();
                }
                return _localResources.InnerBorderPen;
            }
        }

        private LinearGradientBrush LeftDropShadowBrush
        {
            get
            {
                if (!base.IsEnabled)
                {
                    return null;
                }
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return CommonPressedLeftDropShadowBrush;
                    }
                    return null;
                }
                if (_localResources == null)
                {
                    return null;
                }
                if (_localResources.LeftDropShadowBrush == null)
                {
                    _localResources.LeftDropShadowBrush = CommonPressedLeftDropShadowBrush.Clone();
                    _localResources.LeftDropShadowBrush.Opacity = 0.0;
                }
                return _localResources.LeftDropShadowBrush;
            }
        }

        public bool RenderDefaulted
        {
            get
            {
                return (bool)base.GetValue(RenderDefaultedProperty);
            }
            set
            {
                base.SetValue(RenderDefaultedProperty, value);
            }
        }

        public bool RenderMouseOver
        {
            get
            {
                return (bool)base.GetValue(RenderMouseOverProperty);
            }
            set
            {
                base.SetValue(RenderMouseOverProperty, value);
            }
        }

        public bool RenderPressed
        {
            get
            {
                return (bool)base.GetValue(RenderPressedProperty);
            }
            set
            {
                base.SetValue(RenderPressedProperty, value);
            }
        }

        public bool RoundCorners
        {
            get
            {
                return (bool)base.GetValue(RoundCornersProperty);
            }
            set
            {
                base.SetValue(RoundCornersProperty, value);
            }
        }

        public bool DisableInnerBorder
        {
            get
            {
                return (bool)base.GetValue(DisableInnerBorderProperty);
            }
            set
            {
                base.SetValue(DisableInnerBorderProperty, value);
            }
        }

        private LinearGradientBrush TopDropShadowBrush
        {
            get
            {
                if (!base.IsEnabled)
                {
                    return null;
                }
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return CommonPressedTopDropShadowBrush;
                    }
                    return null;
                }
                if (_localResources == null)
                {
                    return null;
                }
                if (_localResources.TopDropShadowBrush == null)
                {
                    _localResources.TopDropShadowBrush = CommonPressedTopDropShadowBrush.Clone();
                    _localResources.TopDropShadowBrush.Opacity = 0.0;
                }
                return _localResources.TopDropShadowBrush;
            }
        }

        private class LocalResources
        {
            public LinearGradientBrush BackgroundOverlay;
            public Pen BorderOverlayPen;
            public Pen InnerBorderPen;
            public LinearGradientBrush LeftDropShadowBrush;
            public LinearGradientBrush TopDropShadowBrush;
        }
    }
}
