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
    /// The Advanced ListView Subitem that can be used in Advanced ListView items.
    /// </summary>
    public class ManagedListViewSubItem : IManagedListViewItem
    {
        string columnID = "";

        /// <summary>
        /// Get or set the column id that this subitem belongs to. This value is important and this subitem will 
        /// NOT GET DROWN until this value is set correctly. Use the same value of ManagedListViewColumn.ID
        /// </summary>
        public string ColumnID
        { get { return columnID; } set { columnID = value; } }
    }
}
