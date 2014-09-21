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
namespace MLV
{
    /// <summary>
    /// Advanced ListView column.
    /// </summary>
    public class ManagedListViewColumn
    {
        private string text = "";
        private string id = "";
        private ManagedListViewSortMode sortMode = ManagedListViewSortMode.None;
        private int width = 60;
        /// <summary>
        /// Get or set the header text of this column.
        /// </summary>
        public string HeaderText
        { get { return text; } set { text = value; } }
        /// <summary>
        /// Get or set the sortmode for this column that will be used to sort items that connected to this column using id.
        /// </summary>
        public ManagedListViewSortMode SortMode
        { get { return sortMode; } set { sortMode = value; } }
        /// <summary>
        /// Get or set the id of this column. Use this to connect subitems to this column.
        /// </summary>
        public string ID
        { get { return id; } set { id = value; } }
        /// <summary>
        /// Get or set the width of this column.
        /// </summary>
        public int Width
        { get { return width; } set { width = value; } }
    }
}
