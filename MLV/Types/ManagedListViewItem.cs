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
using System.Collections.Generic;

namespace MLV
{
    /// <summary>
    /// Advanced ListView item.
    /// </summary>
    public class ManagedListViewItem : IManagedListViewItem
    {
        private List<ManagedListViewSubItem> subItems = new List<ManagedListViewSubItem>();
        private bool selected = false;
        private bool specialItem = false;

        /// <summary>
        /// Get subitem using column id
        /// </summary>
        /// <param name="id">The column id</param>
        /// <returns>The arget subitem if found otherwise null.</returns>
        public ManagedListViewSubItem GetSubItemByID(string id)
        {
            foreach (var subItem in subItems)
            {
                if (subItem.ColumnID == id)
                    return subItem;
            }
            return null;
        }
        /// <summary>
        /// Get or set the subitems collection.
        /// </summary>
        public List<ManagedListViewSubItem> SubItems
        { get { return subItems; } set { subItems = value; } }
        /// <summary>
        /// Get or set a value indecate whether this item is selected.
        /// </summary>
        public bool Selected
        { get { return selected; } set { selected = value; } }
        /// <summary>
        /// Get or set a value indecate whether this item is special. Special items always colered with special color.
        /// </summary>
        public bool IsSpecialItem
        { get { return specialItem; } set { specialItem = value; } }
        /// <summary>
        /// Rises the "on mouse leave" event.
        /// </summary>
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            foreach (IManagedListViewItem subitem in subItems)
                subitem.OnMouseLeave();
        }
    }
}
