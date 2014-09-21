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
using System.Drawing;

namespace MLV
{
    /// <summary>
    /// Arguments for column draw events
    /// </summary>
    public class ManagedListViewColumnDrawArgs : EventArgs
    {
        /// <summary>
        /// Arguments for column draw events
        /// </summary>
        /// <param name="id">The target column id</param>
        /// <param name="gr">The graphics class used to draw the column</param>
        /// <param name="rectangle">The rectangle area of the column in the draw panel</param>
        public ManagedListViewColumnDrawArgs(string id,  Graphics gr, Rectangle rectangle)
        {
            this.id = id;
            this.rectangle = rectangle;
            this.gr = gr;
        }

        private string id = "";
        private Rectangle rectangle;
        private Graphics gr;

        /// <summary>
        /// Get the column id
        /// </summary>
        public string ColumnID
        {
            get { return id; }
        }
        /// <summary>
        /// Get the rectangle area of the column in the draw panel
        /// </summary>
        public Rectangle ColumnRectangle
        {
            get { return rectangle; }
        }
        /// <summary>
        /// Get the graphics class used to draw the column
        /// </summary>
        public Graphics Graphics
        { get { return gr; } }
    }
}
