﻿using MLV;
using System;
﻿using System.Collections.Generic;
using System.Linq;

namespace SkyJukebox
{
    public class ManagedListViewHelper<T>
    {
        private readonly WeakReference<ManagedListView> _reference;
        private ManagedListView _temp;
        /// <summary>
        /// Gets the bound ManagedListView object.
        /// </summary>
        public ManagedListView ManagedListView
        {
            get
            {
                if (_temp == null)
                    _reference.TryGetTarget(out _temp);
                return _temp;
            }
        }
        /// <summary>
        /// Gets the list of Columns.
        /// </summary>
        public List<Column<T>> Columns { get; private set; }
        /// <summary>
        /// Gets the list of Items.
        /// </summary>
        public List<T> Items { get; private set; }
        private ManagedListViewItem MakeListViewItem(T item)
        {
            return new ManagedListViewItem
            {
                //Text = Columns[0].DataFunc(item).ToString(),
                SubItems = (from c in Columns
                            select new ManagedListViewSubItem
                            {
                                Text = c.DataFunc(item).ToString(),
                                ColumnID = Columns.IndexOf(c).ToString()
                            }).ToList()
            };
        }
        private T GetItem(ManagedListViewItem item)
        {
            return Items[ManagedListView.Items.IndexOf(item)];
        }
        /// <summary>
        /// Clears all Items and Columns.
        /// </summary>
        public void ClearAll()
        {
            Items.Clear();
            Columns.Clear();
            ManagedListView.Items.Clear();
            ManagedListView.Columns.Clear();
        }
        /// <summary>
        /// Reloads Items into ManagedListView.
        /// </summary>
        public void RefreshView()
        {
            ManagedListView.Items.Clear();
            foreach (var i in Items)
                ManagedListView.Items.Add(MakeListViewItem(i));
        }
        /// <summary>
        /// Reloads Items and Columns into ManagedListView.
        /// </summary>
        public void RefreshAll()
        {
            // Add column names
            ManagedListView.Columns.Clear();
            foreach (var c in Columns)
                ManagedListView.Columns.Add(new ManagedListViewColumn
                {
                    HeaderText = c.Name,
                    ID = Columns.IndexOf(c).ToString()
                });
            // Add items
            RefreshView();
        }
        public void Add(T item)
        {
            Items.Add(item);
            ManagedListView.Items.Add(MakeListViewItem(item));
        }
        public void AddRange(IEnumerable<T> items)
        {
            var collection = items as IList<T> ?? items.ToList();
            Items.AddRange(collection);
            foreach (var i in collection)
                ManagedListView.Items.Add(MakeListViewItem(i));
        }
        public void Insert(int index, T item)
        {
            Items.Insert(index, item);
            ManagedListView.Items.Insert(index, MakeListViewItem(item));
        }
        private void Insert(int index, ManagedListViewItem item)
        {
            Items.Insert(index, GetItem(item));
            ManagedListView.Items.Insert(index, item);
        }
        public void Remove(T item)
        {
            Items.Remove(item);
            ManagedListView.Items.Remove(MakeListViewItem(item));
        }
        private void Remove(ManagedListViewItem item)
        {
            Items.Remove(GetItem(item));
            ManagedListView.Items.Remove(item);
        }
        public void RemoveSelected()
        {
            foreach (var i in ManagedListView.SelectedItems)
                Remove(i);
        }
        public void RemoveAll()
        {
            Items.Clear();
            ManagedListView.Items.Clear();
        }
        public void Move(T item, int targetIndex)
        {
            Remove(item);
            Insert(targetIndex, item);
        }
        private void Move(ManagedListViewItem item, int targetIndex)
        {
            Remove(item);
            Insert(targetIndex, item);
        }
        public void MoveToTop()
        {
            for (var i = 0; i < ManagedListView.SelectedItems.Count; ++i)
                if (ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) > 0)
                    Move(ManagedListView.SelectedItems[i], i);
        }
        public void MoveToBottom()
        {
            for (var i = ManagedListView.SelectedItems.Count - 1; i > -1; --i)
                if (ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) < ManagedListView.Items.Count - 1)
                    Move(ManagedListView.SelectedItems[i], ManagedListView.Items.Count - (ManagedListView.SelectedItems.Count - i));
        }
        public void MoveUp()
        {
            for (var i = 0; i < ManagedListView.SelectedItems.Count; ++i)
                if (ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) > 0)
                    Move(ManagedListView.SelectedItems[i], ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) - 1);
        }
        public void MoveDown()
        {
            for (var i = ManagedListView.SelectedItems.Count - 1; i > -1; --i)
                if (ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) < ManagedListView.Items.Count - 1)
                    Move(ManagedListView.SelectedItems[i], ManagedListView.Items.IndexOf(ManagedListView.SelectedItems[i]) + 1);
        }
        public void Sort(int columnId)
        {
            ManagedListView.Items.Sort((a, b) => a.SubItems[0].Text.CompareTo(b.SubItems[0].Text));
        }
        public ManagedListViewHelper(ref ManagedListView view)
        {
            Columns = new List<Column<T>>();
            Items = new List<T>();
            _reference = new WeakReference<ManagedListView>(view);
        }
        public ManagedListViewHelper(ref ManagedListView view, IEnumerable<Column<T>> cols)
            : this(ref view)
        {
            Columns.AddRange(cols);
            RefreshAll();
        }
        public ManagedListViewHelper(ref ManagedListView view, IEnumerable<Column<T>> cols, IEnumerable<T> items)
            : this(ref view)
        {
            Columns.AddRange(cols);
            Items.AddRange(items);
            RefreshAll();
        }
        public void SetListView(ref ManagedListView view)
        {
            _reference.SetTarget(view);
            _temp = null;
        }
    }
    public class Column<T> : IComparable<Column<T>>
    {
        public Func<T, object> DataFunc { get; set; }
        public string Name { get; set; }
        public Column() { }
        public Column(string name, Func<T, object> func)
        {
            Name = name;
            DataFunc = func;
        }
        public int CompareTo(Column<T> other)
        {
            return Name.CompareTo(other.Name);
        }
    }

    public interface IHelperInvokeRequired
    {
        void InvokeSomething(Action func);
    }
}