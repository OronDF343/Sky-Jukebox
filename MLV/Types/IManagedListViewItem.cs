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

using System.Drawing;

namespace MLV
{
    /// <summary>
    /// The Advanced ListView item interfaced class.
    /// </summary>
    public abstract class IManagedListViewItem
    {
        private string text = "";
        private Color color = Color.Black;
        private int imageIndex;
        private bool customFontEnabled = false;
        private Font font = new Font("Tahoma", 8, FontStyle.Regular);
        private ManagedListViewItemDrawMode drawMode = ManagedListViewItemDrawMode.Text;
        private object tag;

        /// <summary>
        /// Get or set the item text.
        /// </summary>
        public virtual string Text
        { get { return text; } set { text = value; } }
        /// <summary>
        /// Get or set this item text's color.
        /// </summary>
        public virtual Color Color
        { get { return color; } set { color = value; } }
        /// <summary>
        /// Get or set the image index for this item within the ImagesList collection of the parent control.
        /// </summary>
        public virtual int ImageIndex
        { get { return imageIndex; } set { imageIndex = value; } }
        /// <summary>
        /// Get or set the draw mode for this item.
        /// </summary>
        public ManagedListViewItemDrawMode DrawMode
        { get { return drawMode; } set { drawMode = value; } }
        /// <summary>
        /// Get or set if the custom font enabled for this item. Normally the control draw item texts debending on font value 
        /// of that control, but if this value enabled the control will use the font that specified in CustomFont property 
        /// of thos item.
        /// </summary>
        public bool CustomFontEnabled
        { get { return customFontEnabled; } set { customFontEnabled = value; } }
        /// <summary>
        /// Get or set the custom font which will be used to draw text if this item when the CustomFontEnabled property is true.
        /// </summary>
        public Font CustomFont
        { get { return font; } set { font = value; } }
        /// <summary>
        /// Get or set the tag for this item.
        /// </summary>
        public object Tag
        { get { return tag; } set { tag = value; } }
        /// <summary>
        /// Rises the mouse over event.
        /// </summary>
        /// <param name="mouseLocation">The mouse location within the viewport of the parent listview control.</param>
        /// <param name="charFontSize">The char font size</param>
        public virtual void OnMouseOver(Point mouseLocation, Size charFontSize) { }
        /// <summary>
        /// Rises the mouse click event
        /// </summary>
        /// <param name="mouseLocation">The mouse location within the viewport of the parent listview control.</param>
        /// <param name="charFontSize">The char font size</param>
        /// <param name="itemIndex">The item index or the part item index if this item is a subitem.</param>
        public virtual void OnMouseClick(Point mouseLocation, Size charFontSize, int itemIndex) { }
        /// <summary>
        /// Rises the mouse leave event
        /// </summary>
        public virtual void OnMouseLeave() { }
    }
}
