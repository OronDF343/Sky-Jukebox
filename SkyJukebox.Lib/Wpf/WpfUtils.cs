using System.Windows;
using System.Windows.Media;

namespace SkyJukebox.Lib.Wpf
{
    public static class WpfUtils
    {
        public static TTarget VisualUpwardSearch<TTarget>(this DependencyObject dep) where TTarget : Visual
        {
            while ((dep != null) && !(dep is TTarget) && dep is Visual)
                dep = VisualTreeHelper.GetParent(dep);
            return dep as TTarget;
        }
    }
}
