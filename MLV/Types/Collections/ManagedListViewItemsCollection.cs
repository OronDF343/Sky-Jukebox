/* This file is part of ALV "Advanced ListView" project.
   A custom control which provide advanced list view.

   Copyright © Ala Hadid 2013

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace MLV
{
    /// <summary>
    /// Advanced ListView Items Collection, can rise events when items added, removed ... etc
    /// </summary>
    public class ManagedListViewItemsCollection : ICollection<ManagedListViewItem>
    {
        private List<ManagedListViewItem> items = new List<ManagedListViewItem>();
        //Events
        /// <summary>
        /// Rised whan an item added to the collection
        /// </summary>
        public event EventHandler ItemAdded;
        /// <summary>
        /// Rised when an item removed from the collection
        /// </summary>
        public event EventHandler ItemRemoved;
        /// <summary>
        /// Rised when the collection get cleared
        /// </summary>
        public event EventHandler CollectionClear;
        /// <summary>
        /// Advanced ListView Items Collection
        /// </summary>
        /// <param name="index">The item index</param>
        /// <returns><see cref="ManagedListViewItem"/></returns>
        public ManagedListViewItem this[int index]
        {
            get { if (index < items.Count && index >= 0) return items[index]; else return null; }
            set { if (index < items.Count && index >= 0) items[index] = value; }
        }
        /// <summary>
        /// Add item to the collection
        /// </summary>
        /// <param name="item"><see cref="ManagedListViewItem"/></param>
        public void Add(ManagedListViewItem item)
        {
            items.Add(item);
            if (ItemAdded != null)
                ItemAdded(this, new EventArgs());
        }
        /// <summary>
        /// Clear this collection
        /// </summary>
        public void Clear()
        {
            items.Clear();
            if (CollectionClear != null)
                CollectionClear(this, new EventArgs());
        }
        /// <summary>
        /// Get whether an item exist in this collection
        /// </summary>
        /// <param name="item"><see cref="ManagedListViewItem"/></param>
        /// <returns>True if the item exists otherwise false</returns>
        public bool Contains(ManagedListViewItem item)
        {
            return items.Contains(item);
        }
        /// <summary>
        /// Copy this collection to an array
        /// </summary>
        /// <param name="array">The target array to copy into</param>
        /// <param name="arrayIndex">The index within the target array to start with</param>
        public void CopyTo(ManagedListViewItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Get the items count in this collecion
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }
        /// <summary>
        /// Get whether this collection is read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Remove an item from this collection
        /// </summary>
        /// <param name="item"><see cref="ManagedListViewItem"/> to remove</param>
        /// <returns>True if removed successfuly otherwise false.</returns>
        public bool Remove(ManagedListViewItem item)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new EventArgs());
            return items.Remove(item);
        }
        /// <summary>
        /// Get the index of given item within this collection
        /// </summary>
        /// <param name="item">The item to get index of</param>
        /// <returns>The index of given item if found otherwise false</returns>
        public int IndexOf(ManagedListViewItem item)
        {
            return items.IndexOf(item);
        }
        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator<ManagedListViewItem> IEnumerable<ManagedListViewItem>.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        /// <summary>
        /// Insert item to this collection at given index
        /// </summary>
        /// <param name="index">The index to insert the item at</param>
        /// <param name="item">The item to insert</param>
        public void Insert(int index, ManagedListViewItem item)
        {
            items.Insert(index, item);
            if (ItemAdded != null)
                ItemAdded(this, new EventArgs());
        }
        /// <summary>
        /// Sort the items collection
        /// </summary>
        public void Sort()
        {
            items.Sort();
        }
        /// <summary>
        /// Sort the items collection using a Comparison
        /// </summary>
        /// <param name="comparer">The Comparison to use in compare operation</param>
        public void Sort(Comparison<ManagedListViewItem> comparison)
        {
            items.Sort(comparison);
        }
        /// <summary>
        /// Sort the items collection using Comparer
        /// </summary>
        /// <param name="comparer">The Comparer to use in compare operation</param>
        public void Sort(IComparer<ManagedListViewItem> comparer)
        {
            items.Sort(comparer);
        }
        /// <summary>
        /// Sort the items collection using comparer
        /// </summary>
        /// <param name="index">The start index to start with</param>
        /// <param name="count">The count of items</param>
        /// <param name="comparer">The comparer to use in compare operation</param>
        public void Sort(int index, int count, IComparer<ManagedListViewItem> comparer)
        {
            items.Sort(index, count, comparer);
        }
    }
}
