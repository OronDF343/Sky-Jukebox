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
    /// The columns collection, can rise events when columns added, removed ... etc
    /// </summary>
    public class ManagedListViewColumnsCollection : ICollection<ManagedListViewColumn>
    {
        private List<ManagedListViewColumn> columns = new List<ManagedListViewColumn>();
        //Events
        /// <summary>
        /// Rised when a column added to the collection.
        /// </summary>
        public event EventHandler ColumnAdded;
        /// <summary>
        /// Rised when a column remove from the collection.
        /// </summary>
        public event EventHandler ColumnRemoved;
        /// <summary>
        /// Rised when the collection get cleared.
        /// </summary>
        public event EventHandler CollectionClear;
        /// <summary>
        /// The columns collection.
        /// </summary>
        /// <param name="index">The column index within this collection.</param>
        /// <returns><see cref="ManagedListViewColumn"/></returns>
        public ManagedListViewColumn this[int index]
        { get { return columns[index]; } set { columns[index] = value; } }
        /// <summary>
        /// Add column to this collection
        /// </summary>
        /// <param name="item"><see cref="ManagedListViewColumn"/></param>
        public void Add(ManagedListViewColumn item)
        {
            columns.Add(item);
            if (ColumnAdded != null)
                ColumnAdded(this, new EventArgs());
        }
        /// <summary>
        /// Insert column to this collection
        /// </summary>
        /// <param name="index">The index to insert at</param>
        /// <param name="item"><see cref="ManagedListViewColumn"/></param>
        public void Insert(int index, ManagedListViewColumn item)
        {
            columns.Insert(index, item);
            if (ColumnAdded != null)
                ColumnAdded(this, new EventArgs());
        }
        /// <summary>
        /// Clear this collection
        /// </summary>
        public void Clear()
        {
            columns.Clear();
            if (CollectionClear != null)
                CollectionClear(this, new EventArgs());
        }
        /// <summary>
        /// Get value indecate whether a column exists in this collection.
        /// </summary>
        /// <param name="item"><see cref="ManagedListViewColumn"/></param>
        /// <returns>True if given column exists in this collection otherwise false</returns>
        public bool Contains(ManagedListViewColumn item)
        {
            return columns.Contains(item);
        }
        /// <summary>
        /// Copy this collection to an array.
        /// </summary>
        /// <param name="array">The target array to copy into</param>
        /// <param name="arrayIndex">The index within the target array to start with</param>
        public void CopyTo(ManagedListViewColumn[] array, int arrayIndex)
        {
            columns.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Get columns count in this collection
        /// </summary>
        public int Count
        {
            get { return columns.Count; }
        }
        /// <summary>
        /// Get a value indecate whether this collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Remove a column from this collection
        /// </summary>
        /// <param name="item">The <see cref="ManagedListViewColumn"/> to remove</param>
        /// <returns>True if column removed successfuly otherwise false.</returns>
        public bool Remove(ManagedListViewColumn item)
        {
            if (ColumnRemoved != null)
                ColumnRemoved(this, new EventArgs());
            return columns.Remove(item);
        }
        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns>The enumerator of this collection</returns>
        public IEnumerator<ManagedListViewColumn> GetEnumerator()
        {
            return columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return columns.GetEnumerator();
        }
        /// <summary>
        /// Get column using given id
        /// </summary>
        /// <param name="id">The target column id</param>
        /// <returns>The column if found otherwise null.</returns>
        public ManagedListViewColumn GetColumnByID(string id)
        {
            foreach (var column in columns)
            {
                if (column.ID == id) return column;
            }
            return null;
        }

        /// <summary>
        /// Sort the columns collection
        /// </summary>
        public void Sort()
        {
            columns.Sort();
        }
        /// <summary>
        /// Sort the columns collection using comparer
        /// </summary>
        /// <param name="comparer">The comparer to use in compare operation</param>
        public void Sort(IComparer<ManagedListViewColumn> comparer)
        {
            columns.Sort(comparer);
        }
        /// <summary>
        /// Sort the columns collection using comparer
        /// </summary>
        /// <param name="index">The start index to start with</param>
        /// <param name="count">The count of columns</param>
        /// <param name="comparer">The comparer to use in compare operation</param>
        public void Sort(int index, int count, IComparer<ManagedListViewColumn> comparer)
        {
            columns.Sort(index, count, comparer);
        }
    }
}
