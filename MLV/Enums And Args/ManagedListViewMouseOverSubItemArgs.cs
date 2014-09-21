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

namespace MLV
{
    /// <summary>
    /// Arguments for mouse over subitem events.
    /// </summary>
    public class ManagedListViewMouseOverSubItemArgs : EventArgs
    {
        /// <summary>
        /// Arguments for mouse over subitem events.
        /// </summary>
        /// <param name="itemIndex">The target item index.</param>
        /// <param name="columnID">The column id which the subitem belong to.</param>
        /// <param name="mouseX">The mouse x coordinate value in the panel (not the view port).</param>
        public ManagedListViewMouseOverSubItemArgs(int itemIndex, string columnID, int mouseX)
        {
            this.itemIndex = itemIndex;
            this.columnID = columnID;
            this.mouseX = mouseX;
        }

        private int itemIndex = -1;
        private string columnID = "";
        private int mouseX = 0;

        /// <summary>
        /// Get the column id which the subitem belong to.
        /// </summary>
        public string ColumnID
        { get { return columnID; } }
        /// <summary>
        /// Get the parent item index.
        /// </summary>
        public int ItemIndex
        { get { return itemIndex; } }
        /// <summary>
        /// The mouse x coordinate value in the panel (not the view port).
        /// </summary>
        public int MouseX
        { get { return mouseX; } }
    }
}
