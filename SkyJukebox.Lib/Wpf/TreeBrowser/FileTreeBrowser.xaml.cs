using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ShellDll;
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
            PopulateRootList();
        }

        private void PopulateRootList()
        {
            var temp = new List<string>();
            if (DirectoryUtils.IsWindows8OrHigher)
            {
                try
                {
                    RootList.Add(new FileTreeViewModel(new DirectoryInfoEx(KnownFolderIds.SkyDrive)) { FileExtensionFilter = temp });
                }
                catch { }
            }
            if (DirectoryUtils.IsWindows7OrHigher)
            {
                try
                {
                    RootList.Add(new FileTreeViewModel(new DirectoryInfoEx(KnownFolderIds.Libraries)) { FileExtensionFilter = temp });
                }
                catch { }
            }
            if (DirectoryUtils.IsWindows7OrHigher)
            {
                try
                {
                    RootList.Add(new FileTreeViewModel(new DirectoryInfoEx(KnownFolderIds.HomeGroup)) { FileExtensionFilter = temp });
                }
                catch { }
            }
            RootList.Add(new FileTreeViewModel(new DirectoryInfoEx(KnownFolderIds.UsersFiles)) { FileExtensionFilter = temp });
            RootList.Add(new FileTreeViewModel(DirectoryInfoEx.MyComputerDirectory) { FileExtensionFilter = temp });
            RootList.Add(new FileTreeViewModel(new DirectoryInfoEx(KnownFolderIds.NetworkFolder)) { FileExtensionFilter = temp });
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

        public void Refresh()
        {
            RootList.ForEach(m => m.Refresh());
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch<TreeViewItem>();

            if (treeViewItem != null)
                treeViewItem.Focus();
            base.OnPreviewMouseRightButtonDown(e);
        }

        private Point _startPoint;

        private void FileTreeBrowser_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            _startPoint = e.GetPosition(null);
        }

        private void FileTreeBrowser_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton != MouseButtonState.Pressed ||
                (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                 !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))) return;

            // Get the TreeView
            var treeView = sender as TreeView;
            if (treeView == null) return;
            // Get the selected item
            if (treeView.SelectedItem == null) return;
            var item = treeView.SelectedItem as FileTreeViewModel;
            if (item == null) return;
            // Get the container
            var treeViewItem = ((DependencyObject)e.OriginalSource).VisualUpwardSearch<TreeViewItem>();
            if (treeViewItem == null) return;

            // Initialize the drag & drop operation
            var dragData = new DataObject(DataFormats.FileDrop, new string[] { item.Path.FullName });
            DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
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
