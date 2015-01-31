using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SkyJukebox.Lib.Icons;
using SkyJukebox.Lib.Wpf;

namespace SkyJukebox.Lib.TreeBrowser
{
    /// <summary>
    /// Interaction logic for FileTreeBrowser.xaml
    /// </summary>
    public partial class FileTreeBrowser
    {
        public FileTreeBrowser()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            foreach (var s in Directory.GetLogicalDrives())
            {
                var item = new TreeViewItem { Header = s, Tag = FileSystemInfoEx.FromString(s), FontWeight = FontWeights.Normal };
                item.Items.Add(DummyNode);
                item.Expanded += Folder_Expanded;
                TreeControl.Items.Add(item);
            }
        }

        private static readonly ICollection<string> EmptyFilter = new List<string>();

        public static readonly DependencyProperty FileExtensionFilterProperty =
            DependencyProperty.Register("FileExtensionFilter", typeof(ICollection<string>),
                                        typeof(FileTreeBrowser), new PropertyMetadata(EmptyFilter),
                                        value => value != null);

        public ICollection<string> FileExtensionFilter { get { return (ICollection<string>)GetValue(FileExtensionFilterProperty); } set { SetValue(FileExtensionFilterProperty, value); } } 

        private const object DummyNode = null;

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count < 1 || item.Items[0] != DummyNode) return;
            item.Items.Clear();
            if (!(item.Tag is DirectoryInfoEx)) return;
            var t = item.Tag as DirectoryInfoEx;
            try
            {
                foreach (var s in t.GetDirectories())
                {
                    var subitem = new TreeViewItem
                    {
                        Header = s.Name,
                        Tag = s,
                        FontWeight = FontWeights.Normal
                    };
                    subitem.Items.Add(DummyNode);
                    subitem.Expanded += Folder_Expanded;
                    item.Items.Add(subitem);
                }

                foreach (var f in t.GetFiles().Where(i => FileExtensionFilter.Contains(i.Name.GetExt())))
                {
                    var subitem = new TreeViewItem
                    {
                        Header = f.Name,
                        Tag = f,
                        FontWeight = FontWeights.Normal
                    };
                    item.Items.Add(subitem);
                }
            }
            catch (Exception) { }
        }

        public static string GetPath(object p, out bool isDir)
        {
            isDir = false;
            if (!(p is TreeViewItem)) return null;
            var t = p as TreeViewItem;
            if (!(t.Tag is FileSystemInfoEx)) return null;
            var f = t.Tag as FileSystemInfoEx;
            isDir = f.IsFolder;
            return f.FullName;
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch<TreeViewItem>();

            if (treeViewItem != null)
                treeViewItem.Focus();
            base.OnPreviewMouseRightButtonDown(e);
        }
    }

    public class HeaderToImageConverter : IValueConverter
    {
        public HeaderToImageConverter()
        {
            _iconExtractor = new ExIconExtractor();
        }
        private readonly ExIconExtractor _iconExtractor;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as FileSystemInfoEx;
            return s == null ? null : _iconExtractor.GetIcon(s, IconSize.Small, s.IsFolder, true).ToBitmapSource();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
