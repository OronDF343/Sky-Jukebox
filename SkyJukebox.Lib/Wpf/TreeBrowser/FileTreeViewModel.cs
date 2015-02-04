using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

// Original version by Josh Smith: http://www.codeproject.com/Articles/28306/Working-with-Checkboxes-in-the-WPF-TreeView
// Edited for use with FileSystemInfoEx dynamic enumeration
using System.Windows;

namespace SkyJukebox.Lib.Wpf.TreeBrowser
{
    public enum FilterActions
    {
        Off, Whitelist, Blacklist
    }

    public class FileTreeViewModel : INotifyPropertyChanged
    {
        public FileTreeViewModel(FileSystemInfoEx root)
        {
            FileExtensionFilter = new List<string>();
            Children = new List<FileTreeViewModel>();
            Path = root;
            if (!Path.IsFolder) return;
            _isEnumerationRequired = true;
            Children.Add(null);
        }

        public FileTreeViewModel(FileTreeViewModel parent, FileSystemInfoEx path)
            : this(path)
        {
            _parent = parent;
        }

        #region FSI Enumeration

        private void EnumerateFiles()
        {
            var dp = (Path as DirectoryInfoEx);
            if (dp == null || !_isEnumerationRequired) return;
            _isEnumerationRequired = false;
            Children.Clear();
            Children.AddRange(from fsi in dp.GetFileSystemInfos()
                              where _filterAction == null || (!fsi.IsFolder && !(FileExtensionFilter.Contains(fsi.FullName.GetExt()) ^ (bool)_filterAction)) || fsi.IsFolder
                              select new FileTreeViewModel(this, fsi){ _isChecked = _isChecked == true, FileExtensionFilter = FileExtensionFilter, _filterAction = _filterAction });
        }

        public void OnExpand(object sender, RoutedEventArgs e)
        {
            EnumerateFiles();
        }

        public List<FileSystemInfoEx> GetChecked()
        {
            switch (IsChecked)
            {
                case true:
                    return new List<FileSystemInfoEx>{ Path };
                case false:
                    return new List<FileSystemInfoEx>();
                default:
                    return (from c in Children
                            where c != null
                            from p in c.GetChecked()
                            select p).ToList();
            }
        }

        #endregion

        #region Data

        private bool _isEnumerationRequired;

        private bool? _isChecked = false;
        private readonly FileTreeViewModel _parent;

        #endregion // Data

        #region Properties

        /// <summary>
        /// Sets the mode of the FileExtensionFilter.
        /// true = Whitelist
        /// false = Blacklist
        /// null = Off
        /// </summary>
        private bool? _filterAction;

        public FilterActions FilterAction
        {
            get
            {
                return _filterAction == true
                           ? FilterActions.Whitelist
                           : _filterAction == false ? FilterActions.Blacklist : FilterActions.Off;
            }
            set
            {
                _filterAction = value == FilterActions.Whitelist
                                    ? true
                                    : value == FilterActions.Blacklist ? (bool?)false : null;
            }
        }

        public ICollection<string> FileExtensionFilter { get; set; }

        public FileSystemInfoEx Path { get; private set; }

        public List<FileTreeViewModel> Children { get; private set; }

        public string Name { get { return Path.Name; } }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FileTreeViewModels.  Setting this property to true or false
        /// will set all children to the same check state, setting it to null
        /// will select only files, and setting it to any value will cause
        /// the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren)
                Children.ForEach(c =>
                {
                    if (c == null) return;
                    if (_isChecked.HasValue) c.SetIsChecked(_isChecked, true, false);
                    else if (!c.Path.IsFolder) c.SetIsChecked(true, false, false);
                    else c.SetIsChecked(false, true, false);
                });

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        private void VerifyCheckState()
        {
            bool? state = null;
            for (var i = 0; i < Children.Count; ++i)
            {
                var current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
