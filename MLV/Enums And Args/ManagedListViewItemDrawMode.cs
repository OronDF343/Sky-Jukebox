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
    /// The item draw mode.
    /// </summary>
    public enum ManagedListViewItemDrawMode
    {
        /// <summary>
        /// Draw text only. The text will be taken from Text property of item or subitem.
        /// </summary>
        Text, 
        /// <summary>
        /// Draw image only. The image will be used from ImagesList at given index of ImageIndex property of item or subitem.
        /// </summary>
        Image, 
        /// <summary>
        /// Draw both text and image. The text will be taken from Text property of item or subitem, 
        /// The image will be used from ImagesList at given index of ImageIndex property of item or subitem.
        /// </summary>
        TextAndImage, 
        /// <summary>
        /// Use user resources to draw both text and image. You must use the draw events to draw once this mode chosen.
        /// </summary>
        UserDraw
    }
}
