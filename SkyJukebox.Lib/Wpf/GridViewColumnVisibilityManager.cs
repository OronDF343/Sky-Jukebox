using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SkyJukebox.Lib.Wpf
{
    public class GridViewColumnVisibilityManager
    {
        private static readonly Dictionary<GridViewColumn, double> OriginalColumnWidths =
            new Dictionary<GridViewColumn, double>();

        public static bool GetIsVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisibleProperty);
        }

        public static void SetIsVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisibleProperty, value);
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(GridViewColumnVisibilityManager),
                                                new UIPropertyMetadata(true, OnIsVisibleChanged));
        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        { 
            var gc = d as GridViewColumn; 
            if (gc == null) return;
            if (GetIsVisible(gc) == false)
            {
                OriginalColumnWidths[gc] = gc.Width; 
                gc.Width = 0;
            } 
            else if (gc.Width == 0) 
                gc.Width = OriginalColumnWidths[gc];
        }
    }
}
