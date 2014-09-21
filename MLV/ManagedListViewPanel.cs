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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MLV.Properties;

namespace MLV
{
    /// <summary>
    /// The Advanced ListView panel. This control should be used only on ManagedListView user control.
    /// </summary>
    public partial class ManagedListViewPanel : Control
    {
        /// <summary>
        /// The Advanced ListView panel.
        /// </summary>
        public ManagedListViewPanel()
        {
            InitializeComponent();
            var flag = ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint;
            SetStyle(flag, true);
            AllowDrop = true;

            items.ItemAdded += new EventHandler(items_ItemAdded);
            items.ItemRemoved += new EventHandler(items_ItemAdded);
            columns.ColumnAdded += new EventHandler(items_ItemAdded);
            columns.ColumnRemoved += new EventHandler(items_ItemAdded);
            _StringFormat = new StringFormat();
            columns.CollectionClear += new EventHandler(columns_CollectionClear);
            items.CollectionClear += new EventHandler(columns_CollectionClear);
            _StringFormat = new StringFormat(StringFormatFlags.NoWrap);
            _StringFormat.Trimming = StringTrimming.EllipsisCharacter;
        }

        private StringFormat _StringFormat;
        private Point DownPoint = new Point();
        private Point UpPoint = new Point();
        private Point DownPointAsViewPort = new Point();
        private Point CurrentMousePosition = new Point();
        /// <summary>
        /// The view mode
        /// </summary>
        public ManagedListViewViewMode viewMode = ManagedListViewViewMode.Details;
        /// <summary>
        /// The columns collection
        /// </summary>
        public ManagedListViewColumnsCollection columns = new ManagedListViewColumnsCollection();
        /// <summary>
        /// The items collection
        /// </summary>
        public ManagedListViewItemsCollection items = new ManagedListViewItemsCollection();
        /// <summary>
        /// The images list
        /// </summary>
        public ImageList ImagesList = new ImageList();
        /// <summary>
        /// The selected items collection
        /// </summary>
        public List<ManagedListViewItem> SelectedItems
        {
            get
            {
                var selectedItems = new List<ManagedListViewItem>();
                foreach (var item in items)
                {
                    if (item.Selected)
                    {
                        selectedItems.Add(item);
                    }
                }
                return selectedItems;
            }
        }
        /// <summary>
        /// The horisontal scroll offset
        /// </summary>
        public int HscrollOffset = 0;
        /// <summary>
        /// The vertical scroll offset value
        /// </summary>
        public int VscrollOffset = 0;
        /// <summary>
        /// Rised when values refresh required.
        /// </summary>
        public event EventHandler RefreshValues;
        /// <summary>
        /// Rised when the control requst a scroll values rest
        /// </summary>
        public event EventHandler ClearScrolls;
        /// <summary>
        /// Indecate whether the user can drag and drop items
        /// </summary>
        public bool AllowItemsDragAndDrop = true;
        /// <summary>
        /// Indecate whether the user can reorder the columns.
        /// </summary>
        public bool AllowColumnsReorder = true;
        /// <summary>
        /// Indecate whether the sort mode value of a column get changed when the user click that column.
        /// </summary>
        public bool ChangeColumnSortModeWhenClick = true;
        /// <summary>
        /// The thumbnail height value
        /// </summary>
        public int ThumbnailsHeight = 36;
        /// <summary>
        /// The thumbnails width value
        /// </summary>
        public int ThumbnailsWidth = 36;
        /// <summary>
        /// The item text height
        /// </summary>
        public int itemTextHeight = 28;
        /// <summary>
        /// Indecate whether to draw item highlight when the mouse over that item.
        /// </summary>
        public bool DrawHighlight = true;
        /// <summary>
        /// The item height
        /// </summary>
        public int itemHeight = 15;
        private ManagedListViewMoveType moveType = ManagedListViewMoveType.None;
        private int selectedColumnIndex = -1;
        private bool highlightSelectedColumn = false;
        private bool highlightItemAsOver = false;
        private int overItemSelectedIndex = 0;
        private int selectedItemIndex = 0;
        private int OldoverItemSelectedIndex = 0;
        private int LatestOverItemSelectedIndex = 0;
        private int originalcolumnWidth = 0;
        private int downX = 0;
        private bool isMouseDown = false;
        private int SelectRectanglex;
        private int SelectRectangley;
        private int SelectRectanglex1;
        private int SelectRectangley1;
        private bool DrawSelectRectangle;
        private bool isSecectingItems = false;
        private bool isMovingColumn = false;
        private int currentColumnMovedIndex = 0;
        private int columnHeight = 24;
        private int columnh = 8;
        private int itemh = 6;
        private int highlightSensitive = 6;
        private int spaceBetweenItemsThunmbailsView = 5;

        #region events
        /// <summary>
        /// Rised when the control requests an advance for vertical scroll value
        /// </summary>
        public event EventHandler AdvanceVScrollRequest;
        /// <summary>
        /// Rised when the control requests a reverse for vertical scroll value
        /// </summary>
        public event EventHandler ReverseVScrollRequest;
        /// <summary>
        /// Rised when the control requests a refresh for scroll bars
        /// </summary>
        public event EventHandler RefreshScrollBars;
        /// <summary>
        /// Rised when selected items value changed
        /// </summary>
        public event EventHandler SelectedIndexChanged;
        /// <summary>
        /// Rised when the control needs to draw column
        /// </summary>
        public event EventHandler<ManagedListViewColumnDrawArgs> DrawColumn;
        /// <summary>
        /// Rised when the control needs to draw item
        /// </summary>
        public event EventHandler<ManagedListViewItemDrawArgs> DrawItem;
        /// <summary>
        /// Rised when the control needs to draw subitem
        /// </summary>
        public event EventHandler<ManagedListViewSubItemDrawArgs> DrawSubItem;
        /// <summary>
        /// Rised when the mouse cursor over a subiem.
        /// </summary>
        public event EventHandler<ManagedListViewMouseOverSubItemArgs> MouseOverSubItem;
        /// <summary>
        /// Rised when a column get clicked
        /// </summary>
        public event EventHandler<ManagedListViewColumnClickArgs> ColumnClicked;
        /// <summary>
        /// Rised when an item double click occures
        /// </summary>
        public event EventHandler<ManagedListViewItemDoubleClickArgs> ItemDoubleClick;
        /// <summary>
        /// Rised when the user presses enter after selecting one item.
        /// </summary>
        public event EventHandler EnterPressedOverItem;
        /// <summary>
        /// Rised when the control requests to show the context menu strip
        /// </summary>
        public event EventHandler<MouseEventArgs> ShowContextMenuStrip;
        /// <summary>
        /// Rised when the control requests to switch into the columns context menu strip
        /// </summary>
        public event EventHandler SwitchToColumnsContextMenu;
        /// <summary>
        /// Rised when the control requests to switch into the normal context menu strip
        /// </summary>
        public event EventHandler SwitchToNormalContextMenu;
        /// <summary>
        /// Rised when a column get resized
        /// </summary>
        public event EventHandler AfterColumnResize;
        /// <summary>
        /// Rised when an item get draged
        /// </summary>
        public event EventHandler ItemsDrag;
        /// <summary>
        /// Rised when the control requests to scroll into given item
        /// </summary>
        public event EventHandler<ManagedListViewItemSelectArgs> ScrollToSelectedItemRequest;
        #endregion
        /// <summary>
        /// Get or set the font
        /// </summary>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                if (RefreshScrollBars != null)
                    RefreshScrollBars(this, null);
                Invalidate();
            }
        }
        private enum ManagedListViewMoveType
        {
            None, ColumnVLine, Column
        }
        /// <summary>
        /// Get item index at cursor point
        /// </summary>
        /// <returns>The item index if found otherwise -1</returns>
        public int GetItemIndexAtCursorPoint()
        {
            return GetItemIndexAtPoint(CurrentMousePosition);
        }
        /// <summary>
        /// Get item index at point
        /// </summary>
        /// <param name="location">The location within the viewport</param>
        /// <returns>The item index if found otherwise -1</returns>
        public int GetItemIndexAtPoint(Point location)
        {
            var index = -1;
            if (viewMode == ManagedListViewViewMode.Details)
            {
                var size = CalculateItemsSize();
                var y = location.Y;
                y -= columnHeight;
                if (y > 0 && y < size)
                {
                    index = (VscrollOffset + y) / itemHeight;
                }
            }
            else
            {
                //thunmbnails view select item
                var offset = VscrollOffset % (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                var itemIndex = passedRows * hLines;

                var mouseVlines = (location.Y + offset) / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                var mouseHlines = location.X / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);

                var indexAsMouse = (mouseVlines * hLines) + mouseHlines;
                if (indexAsMouse + itemIndex < items.Count)
                {
                    if (location.X < hLines * (spaceBetweenItemsThunmbailsView + ThumbnailsWidth))
                    {
                        index = indexAsMouse + itemIndex;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// Calculate all columns width
        /// </summary>
        /// <returns>The columns width (all columns)</returns>
        public int CalculateColumnsSize()
        {
            var size = 0;
            foreach (var column in columns)
            {
                size += column.Width;
            }
            return size;
        }
        /// <summary>
        /// Calculate all items size (height). Works with Details view mode only
        /// </summary>
        /// <returns>The height of all items</returns>
        public int CalculateItemsSize()
        {
            var CharSize = TextRenderer.MeasureText("TEST", Font);
            itemHeight = CharSize.Height + itemh;
            return itemHeight * items.Count;
        }
        /// <summary>
        /// Get the height of one item. Works with Details view mode only
        /// </summary>
        /// <returns></returns>
        public int GetItemHeight()
        {
            var CharSize = TextRenderer.MeasureText("TEST", Font);
            return CharSize.Height + itemh;
        }
        /// <summary>
        /// Get vertical scroll value for item
        /// </summary>
        /// <param name="itemIndex">The item index</param>
        /// <returns>The vertical scroll value</returns>
        public int GetVscrollValueForItem(int itemIndex)
        {
            if (viewMode == ManagedListViewViewMode.Details)
            {
                return itemIndex * itemHeight;
            }
            else
            {
                var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                // used too many math calculation to get this lol
                var val = (itemIndex * (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight)) / hLines;
                val -= val % (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                return val;
            }
        }
        /// <summary>
        /// Calculate all items size (height). Works with Thumbnails view mode only
        /// </summary>
        /// <returns></returns>
        public Size CalculateSizeOfItemsAsThumbnails()
        {
            if (items.Count == 0)
                return Size.Empty;
            var w = 0;
            var h = 0;
            var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
            if (vLines == 0) vLines = 1;
            var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
            if (hLines == 0) vLines = 1;

                var itemRows = Math.Ceiling((double)items.Count / hLines);
                h = (int)(itemRows * (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight));

            if ((spaceBetweenItemsThunmbailsView + ThumbnailsWidth) > Width)
                w = (18 + ThumbnailsWidth);

            return new Size(w, h);
        }
        /// <summary>
        /// Rise the key own event
        /// </summary>
        /// <param name="e">The key event arguments</param>
        public void OnKeyDownRised(KeyEventArgs e)
        {
            if (items.Count > 0)
            {
                if (e.KeyCode == Keys.PageUp)
                {
                    // select none
                    foreach (var item in items)
                        item.Selected = false;
                    // select first one
                    items[0].Selected = true;
                    // scroll
                    if (ScrollToSelectedItemRequest != null)
                        ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(0));
                    return;
                }
                else if (e.KeyCode == Keys.PageDown)
                {
                    // select none
                    foreach (var item in items)
                        item.Selected = false;
                    // select first one
                    items[items.Count - 1].Selected = true;
                    // scroll
                    if (ScrollToSelectedItemRequest != null)
                        ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(items.Count - 1));
                    return;
                }
            }
            if (viewMode == ManagedListViewViewMode.Details)
            {
                #region single selection
                if (SelectedItems.Count == 1)
                {
                    var index = items.IndexOf(SelectedItems[0]);
                    if (e.KeyCode == Keys.Down)
                    {
                        if (index + 1 < items.Count)
                        {
                            items[index].Selected = false;
                            items[index + 1].Selected = true;

                            //see if we need to scroll
                            var lines = (Height / itemHeight) - 2;
                            var maxLineIndex = (VscrollOffset / itemHeight) + lines;

                            if (index + 1 > maxLineIndex)
                                if (AdvanceVScrollRequest != null)
                                    AdvanceVScrollRequest(this, null);

                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Up)
                    {
                        if (index - 1 >= 0)
                        {
                            items[index].Selected = false;
                            items[index - 1].Selected = true;

                            var lowLineIndex = (VscrollOffset / itemHeight) + 1;

                            if (index - 1 < lowLineIndex)
                                if (ReverseVScrollRequest != null)
                                    ReverseVScrollRequest(this, null);

                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Return)
                    {
                        if (EnterPressedOverItem != null)
                            EnterPressedOverItem(this, new EventArgs());
                    }
                    else//char ?
                    {
                        var conv = new KeysConverter();
                        for (var i = index + 1; i < items.Count; i++)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                            {
                                if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                                {
                                    items[index].Selected = false;
                                    items[i].Selected = true;
                                    if (ScrollToSelectedItemRequest != null)
                                        ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                    Invalidate();
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion
                #region multi selection
                else if (SelectedItems.Count > 1)
                {
                    var index = items.IndexOf(SelectedItems[0]);
                    var conv = new KeysConverter();
                    for (var i = index + 1; i < items.Count; i++)
                    {
                        if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                            {
                                items[index].Selected = false;
                                items[i].Selected = true;
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                break;
                            }
                        }
                    }
                }
                #endregion
                #region No selection
                else
                {
                    var conv = new KeysConverter();
                    for (var i = 0; i < items.Count; i++)
                    {
                        if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                            {
                                items[i].Selected = true;
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                break;
                            }
                        }
                    }
                }
                #endregion
            }
            else// Thumbnails
            {
                #region single selection
                if (SelectedItems.Count == 1)
                {
                    var index = items.IndexOf(SelectedItems[0]);
                    // in this mode we got 4 directions so calculations may be more complicated
                    if (e.KeyCode == Keys.Right)
                    {
                        if (index + 1 < items.Count)
                        {
                            // advance selection
                            items[index].Selected = false;
                            items[index + 1].Selected = true;

                            // see if the new selected item is under the viewport
                            var vscroll = GetVscrollValueForItem(index + 1);
                            if (Height - vscroll < (ThumbnailsHeight + itemTextHeight))
                            {
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(index + 1));
                            }
                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Left)
                    {
                        if (index - 1 >= 0)
                        {
                            items[index].Selected = false;
                            items[index - 1].Selected = true;

                            var vscroll = GetVscrollValueForItem(index - 1);

                            if (vscroll < VscrollOffset)
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(index - 1));

                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        // find out the index of the item below the selected one
                        var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                        var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var itemIndexOfFirstItemInViewPort = passedRows * hLines;
                        var addit = index - itemIndexOfFirstItemInViewPort;
                        var newIndex = itemIndexOfFirstItemInViewPort + hLines + addit;
                        // now let's see if we can select this one
                        if (newIndex < items.Count)
                        {
                            // advance selection
                            items[index].Selected = false;
                            items[newIndex].Selected = true;

                            // see if the new selected item is under the viewport
                            var vscroll = GetVscrollValueForItem(newIndex);
                            if (Height - vscroll < (ThumbnailsHeight + itemTextHeight))
                            {
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(newIndex));
                            }
                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Up)
                    {
                        // find out the index of the item above the selected one
                        var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                        var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var itemIndexOfFirstItemInViewPort = passedRows * hLines;
                        var addit = index - itemIndexOfFirstItemInViewPort;
                        var newIndex = itemIndexOfFirstItemInViewPort - hLines + addit;
                        // now let's see if we can select this one
                        if (newIndex >= 0)
                        {
                            // advance selection
                            items[index].Selected = false;
                            items[newIndex].Selected = true;

                            // see if the new selected item is under the viewport
                            var vscroll = GetVscrollValueForItem(newIndex);
                            if (vscroll < VscrollOffset)
                            {
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(newIndex));
                            }
                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    else if (e.KeyCode == Keys.Return)
                    {
                        if (EnterPressedOverItem != null)
                            EnterPressedOverItem(this, new EventArgs());
                    }
                    else//char ?
                    {
                        var conv = new KeysConverter();
                        for (var i = index + 1; i < items.Count; i++)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                            {
                                if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                                {
                                    items[index].Selected = false;
                                    items[i].Selected = true;
                                    if (ScrollToSelectedItemRequest != null)
                                        ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion
                #region multi selection
                else if (SelectedItems.Count > 1)
                {
                    var index = items.IndexOf(SelectedItems[0]);
                    var conv = new KeysConverter();
                    for (var i = index + 1; i < items.Count; i++)
                    {
                        if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                            {
                                items[index].Selected = false;
                                items[i].Selected = true;
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                break;
                            }
                        }
                    }
                }
                #endregion
                #region No selection
                else
                {
                    var conv = new KeysConverter();
                    for (var i = 0; i < items.Count; i++)
                    {
                        if (items[i].GetSubItemByID(columns[0].ID).Text.Length > 0)
                        {
                            if (items[i].GetSubItemByID(columns[0].ID).Text.Substring(0, 1) == conv.ConvertToString(e.KeyCode))
                            {
                                items[i].Selected = true;
                                if (ScrollToSelectedItemRequest != null)
                                    ScrollToSelectedItemRequest(this, new ManagedListViewItemSelectArgs(i));
                                break;
                            }
                        }
                    }
                }
                #endregion
            }
            Invalidate();
        }
        /// <summary>
        /// Rise the refresh scroll bars event
        /// </summary>
        public void OnRefreshScrollBars()
        {
            if (RefreshScrollBars != null)
                RefreshScrollBars(this, null);
        }
        /// <summary>
        /// Rise the mouse leave event
        /// </summary>
        public void OnMouseLeaveRise()
        {
            OnMouseLeave(new EventArgs());
        }

        /// <summary>
        /// Rise the paint event
        /// </summary>
        /// <param name="pe"><see cref="PaintEventArgs"/></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (viewMode == ManagedListViewViewMode.Details)
                DrawDetailsView(pe);
            else
                DrawThumbailsView(pe);

            //select rectangle
            if (DrawSelectRectangle)
                pe.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Gray)),
                    SelectRectanglex, SelectRectangley, SelectRectanglex1 - SelectRectanglex, SelectRectangley1 - SelectRectangley);
        }
        /// <summary>
        /// Rise the mouse down event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            DownPoint = e.Location;
            DownPointAsViewPort = new Point(e.X + HscrollOffset, e.Y + VscrollOffset);
            isMouseDown = true;
            if (viewMode == ManagedListViewViewMode.Details)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Y > columnHeight)
                    {
                        if (e.X > CalculateColumnsSize() - HscrollOffset)
                        {
                            DrawSelectRectangle = true;
                        }
                        else if (e.Y > CalculateItemsSize() - VscrollOffset)
                        {
                            DrawSelectRectangle = true;
                        }
                        else
                            DrawSelectRectangle = false;
                    }
                    else
                        DrawSelectRectangle = false;
                }
                else
                    DrawSelectRectangle = false;
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!highlightItemAsOver && overItemSelectedIndex < 0)
                    {
                        DrawSelectRectangle = true;
                    }
                    else
                    {
                        DrawSelectRectangle = false;
                    }
                }
            }

            Invalidate();
        }
        /// <summary>
        /// Rise the mouse up event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            UpPoint = e.Location;
            base.OnMouseUp(e);
            #region resort column

            if (e.Button == MouseButtons.Left)
            {
                if (moveType == ManagedListViewMoveType.ColumnVLine && selectedColumnIndex >= 0)
                {
                    if (AfterColumnResize != null)
                        AfterColumnResize(this, new EventArgs());
                }
                if (moveType == ManagedListViewMoveType.Column && selectedColumnIndex >= 0 && isMovingColumn)
                {
                    //get index
                    var cX = 0;
                    var x = 0;
                    var i = 0;
                    foreach (var column in columns)
                    {
                        cX += column.Width;
                        if (cX >= HscrollOffset)
                        {
                            if (e.X >= (x - HscrollOffset) && e.X <= (cX - HscrollOffset) + 3)
                            {
                                selectedColumnIndex = i;
                            }
                        }
                        i++;
                        x += column.Width;
                        if (x - HscrollOffset > Width)
                            break;
                    }
                    var currentColumn = columns[currentColumnMovedIndex];
                    columns.Remove(columns[currentColumnMovedIndex]);
                    columns.Insert(selectedColumnIndex, currentColumn);
                }
            }
            #endregion
            isMovingColumn = false;
            DrawSelectRectangle = false;
            SelectRectanglex = 0;
            SelectRectangley = 0;
            SelectRectanglex1 = 0;
            SelectRectangley1 = 0;
            isMouseDown = false;
            Invalidate();
        }
        /// <summary>
        /// Rise the mouse move event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = Cursors.Default;
            CurrentMousePosition = e.Location;

            if (viewMode == ManagedListViewViewMode.Details)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (moveType == ManagedListViewMoveType.ColumnVLine && selectedColumnIndex >= 0)
                    {
                        var shift = e.X - downX;
                        Cursor = Cursors.VSplit;
                        columns[selectedColumnIndex].Width = originalcolumnWidth + shift;
                        Invalidate();
                        if (RefreshValues != null)
                            RefreshValues(this, null);
                        return;
                    }
                }
                #region moving column vertical line
                if (e.Y <= columnHeight)
                {
                    if (SwitchToColumnsContextMenu != null)
                        SwitchToColumnsContextMenu(this, new EventArgs());
                    highlightSelectedColumn = true;
                    if (e.Button == MouseButtons.Left)
                    {
                        if (moveType == ManagedListViewMoveType.ColumnVLine && selectedColumnIndex >= 0)
                        {
                            var shift = e.X - downX;
                            Cursor = Cursors.VSplit;
                            columns[selectedColumnIndex].Width = originalcolumnWidth + shift;
                            Invalidate();
                            if (RefreshValues != null)
                                RefreshValues(this, null);
                        }
                        if (AllowColumnsReorder)
                        {
                            if (moveType == ManagedListViewMoveType.Column && selectedColumnIndex >= 0)
                            {
                                currentColumnMovedIndex = selectedColumnIndex;
                                if (e.X > DownPoint.X + 3 || e.X < DownPoint.X - 3)
                                    isMovingColumn = true;
                            }
                        }
                    }
                    else
                    {
                        moveType = ManagedListViewMoveType.None;
                        var cX = 0;
                        var x = 0;
                        var i = 0;
                        foreach (var column in columns)
                        {
                            cX += column.Width;
                            if (cX >= HscrollOffset)
                            {
                                if (e.X >= (x - HscrollOffset) && e.X <= (cX - HscrollOffset) + 3)
                                {
                                    selectedColumnIndex = i;
                                    moveType = ManagedListViewMoveType.Column;
                                }
                                //vertical line select ?
                                var min = cX - HscrollOffset - 3;
                                var max = cX - HscrollOffset + 3;
                                if (e.X >= min && e.X <= max)
                                {
                                    downX = e.X;
                                    originalcolumnWidth = column.Width;
                                    moveType = ManagedListViewMoveType.ColumnVLine;
                                    Cursor = Cursors.VSplit;
                                    break;
                                }
                            }

                            i++;
                            x += column.Width;

                            if (x - HscrollOffset > Width)
                                break;
                        }
                    }
                }
                else
                {
                    if (SwitchToNormalContextMenu != null)
                        SwitchToNormalContextMenu(this, new EventArgs());
                    if (e.Button == MouseButtons.Left)
                    {
                        if (moveType == ManagedListViewMoveType.ColumnVLine && selectedColumnIndex >= 0)
                        {
                            var shift = e.X - downX;
                            Cursor = Cursors.VSplit;
                            columns[selectedColumnIndex].Width = originalcolumnWidth + shift;
                            Invalidate();
                            if (RefreshValues != null)
                                RefreshValues(this, null);
                        }
                        if (AllowColumnsReorder)
                        {
                            if (moveType == ManagedListViewMoveType.Column && selectedColumnIndex >= 0)
                            {
                                currentColumnMovedIndex = selectedColumnIndex;
                                if (e.X > DownPoint.X + 3 || e.X < DownPoint.X - 3)
                                    isMovingColumn = true;
                            }
                        }
                    }
                    else
                    {
                        //clear
                        moveType = ManagedListViewMoveType.None;
                        selectedColumnIndex = -1;
                        highlightSelectedColumn = false;
                        isMovingColumn = false;
                    }
                }
                #endregion
                #region item select
                if (e.Y > columnHeight)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (DrawSelectRectangle)
                        {
                            //draw select rectangle
                            SelectRectanglex = DownPointAsViewPort.X - HscrollOffset;
                            SelectRectangley = DownPointAsViewPort.Y - VscrollOffset;
                            SelectRectanglex1 = e.X;
                            SelectRectangley1 = e.Y;
                            if (SelectRectanglex1 < SelectRectanglex)
                            {
                                SelectRectanglex = e.X;
                                SelectRectanglex1 = DownPointAsViewPort.X - HscrollOffset;
                            }
                            if (SelectRectangley1 < SelectRectangley)
                            {
                                SelectRectangley = e.Y;
                                SelectRectangley1 = DownPointAsViewPort.Y - VscrollOffset;
                            }
                            if (e.Y > Height)
                            {
                                for (var y = 0; y < 10; y++)
                                    if (AdvanceVScrollRequest != null)
                                        AdvanceVScrollRequest(this, null);
                            }

                            //select the items 
                            if (ModifierKeys != Keys.Control)
                            {
                                foreach (var item in items)
                                    item.Selected = false;
                            }
                            if (SelectRectanglex + HscrollOffset < CalculateColumnsSize())
                            {
                                var CharSize = TextRenderer.MeasureText("TEST", Font);

                                isSecectingItems = true;
                                var itemSelected = false;
                                for (var i = VscrollOffset + SelectRectangley; i < VscrollOffset + SelectRectangley1; i++)
                                {
                                    var itemIndex = (i - columnHeight) / itemHeight;
                                    if (itemIndex < items.Count)
                                    {
                                        items[itemIndex].Selected = true; itemSelected = true;
                                    }
                                }
                                if (itemSelected)
                                    if (SelectedIndexChanged != null)
                                        SelectedIndexChanged(this, new EventArgs());
                            }
                        }
                        //drag and drop
                        if (AllowItemsDragAndDrop)
                        {
                            if (e.X > DownPoint.X + 3 || e.X < DownPoint.X - 3 || e.Y > DownPoint.Y + 3 | e.Y < DownPoint.Y - 3)
                            {
                                if (highlightItemAsOver)
                                {
                                    if (overItemSelectedIndex >= 0)
                                    {
                                        if (items[overItemSelectedIndex].Selected)
                                        {
                                            if (ItemsDrag != null)
                                                ItemsDrag(this, new EventArgs());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (e.X < CalculateColumnsSize() - HscrollOffset)
                        {
                            var CharSize = TextRenderer.MeasureText("TEST", Font);
                            var itemIndex = (e.Y + (VscrollOffset - columnHeight) - (itemHeight / highlightSensitive)) / itemHeight;
                            if (itemIndex < items.Count)
                            {
                                highlightItemAsOver = true;
                                overItemSelectedIndex = itemIndex;
                                if (OldoverItemSelectedIndex != itemIndex)
                                {
                                    if (OldoverItemSelectedIndex >= 0 && OldoverItemSelectedIndex < items.Count)
                                        items[OldoverItemSelectedIndex].OnMouseLeave();
                                }
                                OldoverItemSelectedIndex = itemIndex;
                                //rise the event

                                var cX = 0;
                                var x = 0;
                                var i = 0;
                                foreach (var column in columns)
                                {
                                    cX += column.Width;
                                    if (cX >= HscrollOffset)
                                    {
                                        if (cX > e.X + HscrollOffset)
                                        {
                                            if (i < items[itemIndex].SubItems.Count)
                                            {
                                                var sitem = items[itemIndex].GetSubItemByID(column.ID);
                                                if (sitem != null)
                                                {
                                                    sitem.OnMouseOver(new Point(e.X - (x - HscrollOffset), 0), CharSize);
                                                    if (sitem.GetType() == typeof(ManagedListViewRatingSubItem))
                                                    {
                                                        ((ManagedListViewRatingSubItem)sitem).DrawOverImage = true;
                                                    }
                                                }

                                                if (MouseOverSubItem != null)
                                                {
                                                    MouseOverSubItem(this, new ManagedListViewMouseOverSubItemArgs(overItemSelectedIndex,
                                                        columns[i].ID, e.X - x - HscrollOffset));
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    x += column.Width;
                                    i++;
                                    if (x - HscrollOffset > Width)
                                        break;
                                }


                            }
                            else
                            {
                                highlightItemAsOver = false;
                                if (overItemSelectedIndex < items.Count && overItemSelectedIndex >= 0)
                                    items[OldoverItemSelectedIndex].OnMouseLeave();
                                OldoverItemSelectedIndex = overItemSelectedIndex = -1;
                            }
                        }
                        else
                        {
                            highlightItemAsOver = false;
                            if (overItemSelectedIndex < items.Count && overItemSelectedIndex >= 0)
                                items[OldoverItemSelectedIndex].OnMouseLeave();
                            OldoverItemSelectedIndex = overItemSelectedIndex = -1;
                        }
                    }
                }
                if (DrawSelectRectangle)
                {
                    if (e.Y < columnHeight)
                    {
                        for (var y = 0; y < 10; y++)
                            if (ReverseVScrollRequest != null)
                                ReverseVScrollRequest(this, null);
                    }
                }
                #endregion
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (DrawSelectRectangle)
                    {
                        //draw select rectangle
                        SelectRectanglex = DownPointAsViewPort.X - HscrollOffset;
                        SelectRectangley = DownPointAsViewPort.Y - VscrollOffset;
                        SelectRectanglex1 = e.X;
                        SelectRectangley1 = e.Y;
                        if (SelectRectanglex1 < SelectRectanglex)
                        {
                            SelectRectanglex = e.X;
                            SelectRectanglex1 = DownPointAsViewPort.X - HscrollOffset;
                        }
                        if (SelectRectangley1 < SelectRectangley)
                        {
                            SelectRectangley = e.Y;
                            SelectRectangley1 = DownPointAsViewPort.Y - VscrollOffset;
                        }
                        if (e.Y > Height)
                        {
                            for (var y = 0; y < 10; y++)
                                if (AdvanceVScrollRequest != null)
                                    AdvanceVScrollRequest(this, null);
                        }
                        if (e.Y < 0)
                        {
                            for (var y = 0; y < 10; y++)
                                if (ReverseVScrollRequest != null)
                                    ReverseVScrollRequest(this, null);
                        }

                        //select the items 
                        if (ModifierKeys != Keys.Control)
                        {
                            foreach (var item in items)
                                item.Selected = false;
                        }
                        var itemSelected = false;
                        isSecectingItems = true;
                        var offset = VscrollOffset % (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                        var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        var itemIndex = passedRows * hLines;
                        for (var i = VscrollOffset + SelectRectangley; i < VscrollOffset + SelectRectangley1; i++)
                        {
                            for (var j = HscrollOffset + SelectRectanglex; j < HscrollOffset + SelectRectanglex1; j++)
                            {
                                var recVlines = (i + offset) / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                                var recHlines = j / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                                if (recHlines < hLines)
                                {
                                    var indexAsrec = (recVlines * hLines) + recHlines;
                                    if (indexAsrec < items.Count)
                                    { items[indexAsrec].Selected = true; itemSelected = true; }
                                }
                                j += (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                            }
                            i += (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                        }
                        if (itemSelected)
                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, new EventArgs());
                    }
                    //drag and drop
                    if (AllowItemsDragAndDrop)
                    {
                        if (e.X > DownPoint.X + 3 || e.X < DownPoint.X - 3 || e.Y > DownPoint.Y + 3 | e.Y < DownPoint.Y - 3)
                        {
                            if (highlightItemAsOver)
                            {
                                if (overItemSelectedIndex >= 0)
                                {
                                    if (items[overItemSelectedIndex].Selected)
                                    {
                                        if (ItemsDrag != null)
                                            ItemsDrag(this, new EventArgs());
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //thunmbnails view select item
                    var offset = VscrollOffset % (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                    var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                    var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
                    var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                    var itemIndex = passedRows * hLines;

                    var mouseVlines = (e.Y + offset) / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                    var mouseHlines = e.X / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);

                    var indexAsMouse = (mouseVlines * hLines) + mouseHlines;
                    if (indexAsMouse + itemIndex < items.Count)
                    {
                        if (e.X < hLines * (spaceBetweenItemsThunmbailsView + ThumbnailsWidth))
                        {
                            highlightItemAsOver = true;
                            overItemSelectedIndex = indexAsMouse + itemIndex;
                        }
                        else
                        {
                            highlightItemAsOver = false;
                            OldoverItemSelectedIndex = overItemSelectedIndex = -1;
                        }
                    }
                    else
                    {
                        highlightItemAsOver = false;
                        OldoverItemSelectedIndex = overItemSelectedIndex = -1;
                    }
                }
            }
            Invalidate();
        }
        /// <summary>
        /// Rise the mouse click event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (viewMode == ManagedListViewViewMode.Details)
            {
                #region item select
                if (e.Y > columnHeight)
                {
                    if ((e.X < CalculateColumnsSize() - HscrollOffset))
                    {
                        if (e.Button == MouseButtons.Left && !isSecectingItems && overItemSelectedIndex >= 0)
                        {
                            var currentItemStatus = items[overItemSelectedIndex].Selected;
                            var isShiftSelection = false;
                            if (ModifierKeys == Keys.Shift)
                            {
                                isShiftSelection = true;
                                if (LatestOverItemSelectedIndex == -1)
                                    isShiftSelection = false;
                            }
                            else if (ModifierKeys != Keys.Control)
                            {
                                LatestOverItemSelectedIndex = -1;
                                foreach (var item in items)
                                    item.Selected = false;
                            }

                            if (highlightItemAsOver)
                            {
                                if (overItemSelectedIndex >= 0)
                                {
                                    if (!isShiftSelection)
                                    {
                                        items[overItemSelectedIndex].Selected = true;
                                        LatestOverItemSelectedIndex = overItemSelectedIndex;
                                        if (SelectedIndexChanged != null && !currentItemStatus)
                                            SelectedIndexChanged(this, new EventArgs());

                                        var cX = 0;
                                        var x = 0;
                                        var i = 0;
                                        foreach (var column in columns)
                                        {
                                            cX += column.Width;
                                            if (cX >= HscrollOffset)
                                            {
                                                if (cX > e.X)
                                                {
                                                    var sub = items[overItemSelectedIndex].GetSubItemByID(column.ID);
                                                    if (sub != null)
                                                    {
                                                        sub.OnMouseClick(new Point(e.X - x - HscrollOffset, 0),
                                                            TextRenderer.MeasureText("TEST", Font), overItemSelectedIndex);
                                                    }
                                                    break;
                                                }
                                            }
                                            x += column.Width;
                                            i++;
                                            if (x - HscrollOffset > Width)
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in items)
                                            item.Selected = false;
                                        if (overItemSelectedIndex > LatestOverItemSelectedIndex)
                                        {
                                            for (var j = LatestOverItemSelectedIndex; j < overItemSelectedIndex + 1; j++)
                                            {
                                                items[j].Selected = true;
                                            }
                                            if (SelectedIndexChanged != null)
                                                SelectedIndexChanged(this, new EventArgs());
                                        }
                                        else if (overItemSelectedIndex < LatestOverItemSelectedIndex)
                                        {
                                            for (var j = overItemSelectedIndex; j < LatestOverItemSelectedIndex + 1; j++)
                                            {
                                                items[j].Selected = true;
                                            }
                                            if (SelectedIndexChanged != null)
                                                SelectedIndexChanged(this, new EventArgs());
                                        }
                                    }
                                }

                            }
                        }
                        if (e.Button == MouseButtons.Left && !isSecectingItems && overItemSelectedIndex == -1)
                        {
                            LatestOverItemSelectedIndex = -1;
                            foreach (var item in items)
                                item.Selected = false;
                        }
                    }
                    else
                    {
                        if (!isSecectingItems)
                            foreach (var item in items)
                                item.Selected = false;
                    }
                }
                #endregion
                #region Column click
                if (e.Y < columnHeight)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (moveType == ManagedListViewMoveType.Column && selectedColumnIndex >= 0 && !isMovingColumn)
                        {
                            if (ChangeColumnSortModeWhenClick)
                            {
                                switch (columns[selectedColumnIndex].SortMode)
                                {
                                    case ManagedListViewSortMode.None: columns[selectedColumnIndex].SortMode = ManagedListViewSortMode.AtoZ; break;
                                    case ManagedListViewSortMode.AtoZ: columns[selectedColumnIndex].SortMode = ManagedListViewSortMode.ZtoA; break;
                                    case ManagedListViewSortMode.ZtoA: columns[selectedColumnIndex].SortMode = ManagedListViewSortMode.None; break;
                                }
                            }
                            if (ColumnClicked != null)
                                ColumnClicked(this, new ManagedListViewColumnClickArgs(columns[selectedColumnIndex].ID));
                            Invalidate();
                        }
                    }
                }
                #endregion
            }
            else
            {
                if (e.Button == MouseButtons.Left && !isSecectingItems && overItemSelectedIndex >= 0)
                {
                    var currentItemStatus = items[overItemSelectedIndex].Selected;
                    var isShiftSelection = false;
                    if (ModifierKeys == Keys.Shift)
                    {
                        isShiftSelection = true;
                        if (LatestOverItemSelectedIndex == -1)
                            isShiftSelection = false;
                    }
                    else if (ModifierKeys != Keys.Control)
                    {
                        LatestOverItemSelectedIndex = -1;
                        foreach (var item in items)
                            item.Selected = false;
                    }

                    if (highlightItemAsOver)
                    {
                        if (overItemSelectedIndex >= 0)
                        {
                            if (!isShiftSelection)
                            {
                                items[overItemSelectedIndex].Selected = true;
                                LatestOverItemSelectedIndex = overItemSelectedIndex;
                                if (SelectedIndexChanged != null && !currentItemStatus)
                                    SelectedIndexChanged(this, new EventArgs());
                            }
                            else
                            {
                                foreach (var item in items)
                                    item.Selected = false;
                                if (overItemSelectedIndex > LatestOverItemSelectedIndex)
                                {
                                    for (var j = LatestOverItemSelectedIndex; j < overItemSelectedIndex + 1; j++)
                                    {
                                        items[j].Selected = true;
                                    }
                                    if (SelectedIndexChanged != null)
                                        SelectedIndexChanged(this, new EventArgs());
                                }
                                else if (overItemSelectedIndex < LatestOverItemSelectedIndex)
                                {
                                    for (var j = overItemSelectedIndex; j < LatestOverItemSelectedIndex + 1; j++)
                                    {
                                        items[j].Selected = true;
                                    }
                                    if (SelectedIndexChanged != null)
                                        SelectedIndexChanged(this, new EventArgs());
                                }
                            }
                        }

                    }
                }
                if (e.Button == MouseButtons.Left && !isSecectingItems && overItemSelectedIndex == -1)
                {
                    LatestOverItemSelectedIndex = -1;
                    foreach (var item in items)
                        item.Selected = false;
                }
            }
            if (!Focused)
            {
                base.Focus();
            }
            isSecectingItems = false;
        }
        /// <summary>
        /// Rise the mouse wheel event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (ReverseVScrollRequest != null)
                    ReverseVScrollRequest(this, null);
            }
            if (e.Delta < 0)
            {
                if (AdvanceVScrollRequest != null)
                    AdvanceVScrollRequest(this, null);
            }
            base.OnMouseWheel(e);
            Invalidate();
        }
        /// <summary>
        /// Rise the mouse double click event
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Y > columnHeight)
            {
                if (ItemDoubleClick != null)
                    ItemDoubleClick(this, new ManagedListViewItemDoubleClickArgs(overItemSelectedIndex));
            }
        }
        /// <summary>
        /// Rise the mouse leave event
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (highlightItemAsOver)
            {
                highlightItemAsOver = false;
                if (overItemSelectedIndex < items.Count && overItemSelectedIndex >= 0)
                    if (items[OldoverItemSelectedIndex] != null)
                        items[OldoverItemSelectedIndex].OnMouseLeave();
                OldoverItemSelectedIndex = overItemSelectedIndex = -1;
                Invalidate();
            }
        }

        private void DrawDetailsView(PaintEventArgs pe)
        {
            var cX = 0;
            var x = 0;
            var i = 0;
            //get default size of word
            var CharSize = TextRenderer.MeasureText("TEST", Font);
            columnHeight = CharSize.Height + columnh;
            var columnTextOffset = columnh / 2;
            itemHeight = CharSize.Height + itemh;
            var itemTextOffset = itemh / 2;
            var lines = (Height / itemHeight) + 2;

            var offset = VscrollOffset % itemHeight;

            foreach (var column in columns)
            {
                cX += column.Width;
                if (cX >= HscrollOffset)
                {
                    var lineIndex = VscrollOffset / itemHeight;
                    //draw sub items releated to this column
                    for (var j = 0; j < lines; j++)
                    {
                        try
                        {
                            if (lineIndex < items.Count)
                            {
                                //clear
                                pe.Graphics.FillRectangle(new SolidBrush(base.BackColor),
                                   new Rectangle(x - HscrollOffset + 1, (j * itemHeight) - offset + columnHeight + 1,
                                       column.Width - 1, itemHeight));
                                if (items[lineIndex].IsSpecialItem)
                                {
                                    pe.Graphics.FillRectangle(Brushes.YellowGreen,
                                    new Rectangle(x - HscrollOffset, (j * itemHeight) - offset + columnHeight + 1,
                                        column.Width - 1, itemHeight));
                                }
                                if (items[lineIndex].Selected)
                                {
                                    pe.Graphics.FillRectangle(Brushes.LightSkyBlue,
                                      new Rectangle(x - HscrollOffset, (j * itemHeight) - offset + columnHeight + 1,
                                          column.Width - 1, itemHeight));
                                }
                                else
                                {
                                    if (highlightItemAsOver)
                                    {
                                        if (lineIndex == overItemSelectedIndex)
                                        {
                                            if (DrawHighlight)
                                                pe.Graphics.FillRectangle(Brushes.LightGray,
                                                new Rectangle(x - HscrollOffset, (j * itemHeight) - offset + columnHeight + 1,
                                                    column.Width - 1, itemHeight));
                                        }
                                    }
                                }

                                var subitem = items[lineIndex].GetSubItemByID(column.ID);
                                var drawColor = subitem.Color;
                                var drawFont = subitem.CustomFontEnabled ? subitem.CustomFont : Font;
                                if (subitem != null)
                                {
                                    if (subitem.GetType() == typeof(ManagedListViewRatingSubItem))
                                    {
                                        ((ManagedListViewRatingSubItem)subitem).OnRefreshRating(lineIndex, itemHeight);
                                        Image img = Resources.noneRating;
                                        if (!((ManagedListViewRatingSubItem)subitem).DrawOverImage)
                                        {
                                            switch (((ManagedListViewRatingSubItem)subitem).Rating)
                                            {
                                                case 1: img = Resources.star_1; break;
                                                case 2: img = Resources.star_2; break;
                                                case 3: img = Resources.star_3; break;
                                                case 4: img = Resources.star_4; break;
                                                case 5: img = Resources.star_5; break;
                                            }
                                        }
                                        else
                                        {
                                            switch (((ManagedListViewRatingSubItem)subitem).OverRating)
                                            {
                                                case 1: img = Resources.star_1; break;
                                                case 2: img = Resources.star_2; break;
                                                case 3: img = Resources.star_3; break;
                                                case 4: img = Resources.star_4; break;
                                                case 5: img = Resources.star_5; break;
                                            }
                                        }
                                        pe.Graphics.DrawImage(img,
                                        new Rectangle(x - HscrollOffset + 2, (j * itemHeight) - offset + columnHeight + 1,
                                        itemHeight * 4, itemHeight - 1));

                                        ((ManagedListViewRatingSubItem)subitem).DrawOverImage = false;
                                    }
                                    else
                                    {
                                        switch (subitem.DrawMode)
                                        {
                                            case ManagedListViewItemDrawMode.Text:
                                                pe.Graphics.DrawString(subitem.Text, drawFont, new SolidBrush(drawColor),
                                                    new Rectangle(x - HscrollOffset + 2,
                                                         (j * itemHeight) - offset + columnHeight + itemTextOffset,
                                                        column.Width, CharSize.Height), _StringFormat);
                                                break;
                                            case ManagedListViewItemDrawMode.Image:
                                                if (subitem.ImageIndex < ImagesList.Images.Count)
                                                {
                                                    pe.Graphics.DrawImage(ImagesList.Images[subitem.ImageIndex],
                                                        new Rectangle(x - HscrollOffset + 2, (j * itemHeight) - offset + columnHeight + 1,
                                                            itemHeight - 1, itemHeight - 1));
                                                }
                                                break;
                                            case ManagedListViewItemDrawMode.TextAndImage:
                                                var plus = 2;
                                                if (subitem.ImageIndex < ImagesList.Images.Count)
                                                {
                                                    pe.Graphics.DrawImage(ImagesList.Images[subitem.ImageIndex],
                                                      new Rectangle(x - HscrollOffset + 2, (j * itemHeight) - offset + columnHeight + 1,
                                                          itemHeight - 1, itemHeight - 1));
                                                    plus += itemHeight;
                                                }
                                                pe.Graphics.DrawString(subitem.Text, drawFont, new SolidBrush(drawColor),
                                                    new Rectangle(x - HscrollOffset + 2 + plus,
                                                         (j * itemHeight) - offset + columnHeight + itemTextOffset,
                                                        column.Width - plus, CharSize.Height), _StringFormat);
                                                break;
                                            case ManagedListViewItemDrawMode.UserDraw:    //rise the event
                                                if (DrawSubItem != null)
                                                {
                                                    var args = new ManagedListViewSubItemDrawArgs(column.ID, lineIndex);
                                                    DrawSubItem(this, args);
                                                    var p = 2;
                                                    if (args.ImageToDraw != null)
                                                    {
                                                        pe.Graphics.DrawImage(args.ImageToDraw,
                                                            new Rectangle(x - HscrollOffset + 2, (j * itemHeight) - offset + columnHeight + 1
                                                                , itemHeight - 1, itemHeight - 1));
                                                        p += itemHeight;
                                                    }
                                                    pe.Graphics.DrawString(args.TextToDraw, drawFont, new SolidBrush(drawColor),
                                                    new Rectangle(x - HscrollOffset + 2 + p,
                                                         (j * itemHeight) - offset + columnHeight + itemTextOffset,
                                                        column.Width - p, CharSize.Height), _StringFormat);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message+"\n\n"+ex.ToString()); }
                        lineIndex++;
                    }

                    //draw the column recangle, draw the column after the item to hide the offset
                    var Hcolor = Color.Silver;
                    if (highlightSelectedColumn && selectedColumnIndex == i)
                    {
                        if (!isMouseDown)
                            Hcolor = Color.LightSkyBlue;
                        else
                        {
                            if (moveType != ManagedListViewMoveType.ColumnVLine)
                                Hcolor = Color.PaleVioletRed;
                            else
                                Hcolor = Color.LightSkyBlue;
                        }
                    }
                    //DRAW COLUMN
                    pe.Graphics.FillRectangle(new LinearGradientBrush(new Point(), new Point(0, columnHeight), Color.White, Hcolor),
                            new Rectangle(x - HscrollOffset + 1, 1, column.Width, columnHeight));
                    //draw the column line
                    pe.Graphics.DrawLine(new Pen(Brushes.LightGray), new Point(cX - HscrollOffset, 1),
                        new Point(cX - HscrollOffset, Height - 1));
                    //draw the column text
                    pe.Graphics.DrawString(column.HeaderText, Font,
                        new SolidBrush(Color.Black), new Rectangle(x - HscrollOffset + 2, columnTextOffset, column.Width,
                            columnHeight), _StringFormat);
                    //rise the event
                    if (DrawColumn != null)
                        DrawColumn(this, new ManagedListViewColumnDrawArgs(column.ID, pe.Graphics,
                            new Rectangle(x - HscrollOffset, 2, column.Width, columnHeight)));
                    //draw sort traingle
                    switch (column.SortMode)
                    {
                        case ManagedListViewSortMode.AtoZ:
                            pe.Graphics.DrawImage(Resources.SortAlpha.ToBitmap(),
                                new Rectangle(x - HscrollOffset + column.Width - 14, 2, 12, 16));
                            break;
                        case ManagedListViewSortMode.ZtoA:
                            pe.Graphics.DrawImage(Resources.SortZ.ToBitmap(),
                                new Rectangle(x - HscrollOffset + column.Width - 14, 2, 12, 16));
                            break;
                    }
                }
                x += column.Width;
                i++;
                if (x - HscrollOffset > Width)
                    break;
            }
            if (columns.Count > 0)
                pe.Graphics.DrawLine(new Pen(new SolidBrush(Color.Gray)), new Point(0, columnHeight), new Point(Width, columnHeight));
            if (isMovingColumn)
            {
                pe.Graphics.FillRectangle(new LinearGradientBrush(new Point(), new Point(0, columnHeight), Color.White, Color.Silver),
                           new Rectangle(CurrentMousePosition.X, 1, columns[selectedColumnIndex].Width, columnHeight));
                //draw the column line
                pe.Graphics.DrawLine(new Pen(Brushes.LightGray), new Point(cX - HscrollOffset, 1),
                    new Point(cX - HscrollOffset, Height - 1));
                //draw the column text
                pe.Graphics.DrawString(columns[selectedColumnIndex].HeaderText, Font,
                    new SolidBrush(Color.Black), new Rectangle(CurrentMousePosition.X, 2,
                        columns[selectedColumnIndex].Width, columnHeight), _StringFormat);
            }
        }
        private void DrawThumbailsView(PaintEventArgs pe)
        {
            var CharSize = TextRenderer.MeasureText("TEST", Font);
            itemTextHeight = CharSize.Height * 2;

            var vLines = Height / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
            var hLines = Width / (spaceBetweenItemsThunmbailsView + ThumbnailsWidth);
            if (hLines == 0) hLines = 1;
            var passedRows = VscrollOffset / (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
            var itemIndex = passedRows * hLines;
            if (itemIndex >= items.Count)
                return;
            var y = 2;
            for (var i = 0; i < vLines + 2; i++)
            {
                var x = spaceBetweenItemsThunmbailsView;
                for (var j = 0; j < hLines; j++)
                {
                    var offset = VscrollOffset % (spaceBetweenItemsThunmbailsView + ThumbnailsHeight + itemTextHeight);
                    if (highlightItemAsOver)
                    {
                        if (itemIndex == overItemSelectedIndex)
                        {
                            pe.Graphics.FillRectangle(Brushes.LightGray,
                                new Rectangle(x - 2, y - offset - 2, ThumbnailsWidth + 4, ThumbnailsHeight + itemTextHeight + 4));
                        }
                    }
                    if (items[itemIndex].Selected)
                        pe.Graphics.FillRectangle(Brushes.LightSkyBlue,
                            new Rectangle(x - 2, y - offset - 2, ThumbnailsWidth + 4, ThumbnailsHeight + itemTextHeight + 4));

                    var format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    //format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    var textToDraw = "";
                    Image imageToDraw = null;

                    switch (items[itemIndex].DrawMode)
                    {
                        case ManagedListViewItemDrawMode.Text:
                        case ManagedListViewItemDrawMode.Image:
                        case ManagedListViewItemDrawMode.TextAndImage:
                            if (items[itemIndex].ImageIndex < ImagesList.Images.Count)
                            {
                                imageToDraw = ImagesList.Images[items[itemIndex].ImageIndex];
                            }
                            textToDraw = items[itemIndex].Text;
                            break;

                        case ManagedListViewItemDrawMode.UserDraw:
                            var args = new ManagedListViewItemDrawArgs(itemIndex);
                            if (DrawItem != null)
                                DrawItem(this, args);
                            imageToDraw = args.ImageToDraw;
                            textToDraw = args.TextToDraw;
                            break;
                    }
                    // Draw image
                    if (imageToDraw != null)
                    {
                        var siz = CalculateStretchImageValues(imageToDraw.Width, imageToDraw.Height);
                        var imgX = x + (ThumbnailsWidth / 2) - (siz.Width / 2);
                        var imgY = (y - offset) + (ThumbnailsHeight / 2) - (siz.Height / 2);
                        pe.Graphics.DrawImage(imageToDraw, new Rectangle(imgX, imgY, siz.Width, siz.Height));
                    }
                    // Draw text
                    pe.Graphics.DrawString(textToDraw, Font, Brushes.Black,
                         new Rectangle(x, y + ThumbnailsHeight + 1 - offset, ThumbnailsWidth, itemTextHeight), format);
                    // advance
                    x += ThumbnailsWidth + spaceBetweenItemsThunmbailsView;
                    itemIndex++;
                    if (itemIndex == items.Count)
                        break;
                }
                y += ThumbnailsHeight + itemTextHeight + spaceBetweenItemsThunmbailsView;
                if (itemIndex == items.Count)
                    break;
            }
        }
        private Size CalculateStretchImageValues(int imgW, int imgH)
        {
            var pRatio = (float)ThumbnailsWidth / ThumbnailsHeight;
            var imRatio = (float)imgW / imgH;
            var viewImageWidth = 0;
            var viewImageHeight = 0;

            if (ThumbnailsWidth >= imgW && ThumbnailsHeight >= imgH)
            {
                viewImageWidth = imgW;
                viewImageHeight = imgH;
            }
            else if (ThumbnailsWidth > imgW && ThumbnailsHeight < imgH)
            {
                viewImageHeight = ThumbnailsHeight;
                viewImageWidth = (int)(ThumbnailsHeight * imRatio);
            }
            else if (ThumbnailsWidth < imgW && ThumbnailsHeight > imgH)
            {
                viewImageWidth = ThumbnailsWidth;
                viewImageHeight = (int)(ThumbnailsWidth / imRatio);
            }
            else if (ThumbnailsWidth < imgW && ThumbnailsHeight < imgH)
            {
                if (ThumbnailsWidth >= ThumbnailsHeight)
                {
                    //width image
                    if (imgW >= imgH && imRatio >= pRatio)
                    {
                        viewImageWidth = ThumbnailsWidth;
                        viewImageHeight = (int)(ThumbnailsWidth / imRatio);
                    }
                    else
                    {
                        viewImageHeight = ThumbnailsHeight;
                        viewImageWidth = (int)(ThumbnailsHeight * imRatio);
                    }
                }
                else
                {
                    if (imgW < imgH && imRatio < pRatio)
                    {
                        viewImageHeight = ThumbnailsHeight;
                        viewImageWidth = (int)(ThumbnailsHeight * imRatio);
                    }
                    else
                    {
                        viewImageWidth = ThumbnailsWidth;
                        viewImageHeight = (int)(ThumbnailsWidth / imRatio);
                    }
                }
            }

            return new Size(viewImageWidth, viewImageHeight);
        }
        private void items_ItemAdded(object sender, EventArgs e)
        {
            OnRefreshScrollBars();
            Invalidate();
        }
        private void columns_CollectionClear(object sender, EventArgs e)
        {
            if (ClearScrolls != null)
                ClearScrolls(this, null);

            highlightSelectedColumn = false;
            highlightItemAsOver = false;
            overItemSelectedIndex = -1;
            OldoverItemSelectedIndex = -1;
            LatestOverItemSelectedIndex = -1;
            HscrollOffset = 0;
            VscrollOffset = 0;

            OnRefreshScrollBars();

            Invalidate();
        }
    }
}
