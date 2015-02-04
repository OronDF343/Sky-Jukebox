using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SkyJukebox.Lib.Icons;

namespace SkyJukebox.Lib.Wpf.TreeBrowser
{
    /// <summary>
    /// Interaction logic for FileTreeBrowser.xaml
    /// </summary>
    public partial class FileTreeBrowser
    {
        public FileTreeBrowser()
        {
            RootList = new List<FileTreeViewModel>();
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            var temp = new List<string>();
            foreach (var s in Directory.GetLogicalDrives())
            {
                //var item = new TreeViewItem { Header = s, Tag = FileSystemInfoEx.FromString(s), FontWeight = FontWeights.Normal };
                //item.Items.Add(DummyNode);
                //item.Expanded += Folder_Expanded;
                //TreeControl.Items.Add(item);
                RootList.Add(new FileTreeViewModel(FileSystemInfoEx.FromString(s)){ FileExtensionFilter = temp });
            }
        }

        /// <summary>
        /// Sets the mode of the FileExtensionFilter.
        /// true = Whitelist
        /// false = Blacklist
        /// null = Off
        /// </summary>
        public FilterActions FilterAction
        {
            get { return RootList[0].FilterAction; }
            set { RootList.ForEach(r => r.FilterAction = value); }
        }

        public List<FileTreeViewModel> RootList { get; private set; }

        private void OnExpand(object sender, RoutedEventArgs e)
        {
            var s = sender as TreeViewItem;
            if (s == null) return;
            var model = s.Header as FileTreeViewModel;
            if (model != null) model.OnExpand(sender, e);
        }

        public ICollection<string> FileExtensionFilter
        {
            get { return RootList[0].FileExtensionFilter; }
            set { RootList.ForEach(r => r.FileExtensionFilter = value ?? new List<string>()); }
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
