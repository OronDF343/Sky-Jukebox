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
    /// Argumnets for rating events.
    /// </summary>
    public class ManagedListViewRatingChangedArgs : EventArgs
    {
     
        /// <summary>
        /// Argumnets for rating events.
        /// </summary>
        /// <param name="id">The column id which the subitem belongs to.</param>
        /// <param name="itemIndex">The parent item index</param>
        /// <param name="rating">The rating value (0-5)</param>
        public ManagedListViewRatingChangedArgs(string id, int itemIndex, int rating)
        {
            this.itemIndex = itemIndex;
            this.id = id;
            this.rating = rating;
        }

        private string id = "";
        private int itemIndex;
        private int rating = 0;
        /// <summary>
        /// Get the column id which the subitem belongs to.
        /// </summary>
        public string ColumnID
        { get { return id; } }
        /// <summary>
        /// Get The parent item index
        /// </summary>
        public int ItemIndex
        { get { return itemIndex; } }
        /// <summary>
        /// Get the rating value (0-5; 0=none rating, 5=top rating or 5 stars)
        /// </summary>
        public int Rating
        { get { return rating; } }
    }
}
