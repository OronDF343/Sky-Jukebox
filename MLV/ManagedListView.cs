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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MLV
{
    /// <summary>
    /// Advanced ListView control.
    /// </summary>
    public partial class ManagedListView : UserControl
    {
        /// <summary>
        /// Advanced ListView control.
        /// </summary>
        public ManagedListView()
        {
            InitializeComponent();
            wheelScrollSpeed = ManagedListViewPanel1.GetItemHeight();
        }

        private ManagedListViewViewMode viewMode = ManagedListViewViewMode.Details;
        private int wheelScrollSpeed = 20;

        #region properties
        /// <summary>
        /// Get or set the viewmode.
        /// </summary>
        [Description("The list view mode"), Category("ManagedListView")]
        public ManagedListViewViewMode ViewMode
        {
            get { return viewMode; }
            set
            {
                viewMode = value;
                ManagedListViewPanel1.viewMode = value;
                ManagedListViewPanel1.HscrollOffset = hScrollBar1.Value = 0;
                ManagedListViewPanel1.VscrollOffset = vScrollBar1.Value = 0;
                ManagedListViewPanel1_RefreshScrollBars(this, null);
                ManagedListViewPanel1.Invalidate();

                if (ViewModeChanged != null)
                    ViewModeChanged(this, new EventArgs());

                if (value == ManagedListViewViewMode.Thumbnails)
                    if (SwitchToNormalContextMenu != null)
                        SwitchToNormalContextMenu(this, new EventArgs());
            }
        }
        /// <summary>
        /// Get or set the column collection.
        /// </summary>
        [Description("The columns collection"), Category("ManagedListView")]
        public ManagedListViewColumnsCollection Columns
        {
            get { return ManagedListViewPanel1.columns; }
            set { ManagedListViewPanel1.columns = value; ManagedListViewPanel1.Invalidate(); }
        }
        /// <summary>
        /// Get or set the items collection.
        /// </summary>
        [Description("The items collection"), Category("ManagedListView")]
        public ManagedListViewItemsCollection Items
        {
            get { return ManagedListViewPanel1.items; }
            set { ManagedListViewPanel1.items = value; ManagedListViewPanel1.Invalidate(); }
        }
        /// <summary>
        /// Get or set if selected items can be draged and droped
        /// </summary>
        [Description("If enabled, the selected items can be draged and droped"), Category("ManagedListView")]
        public bool AllowItemsDragAndDrop
        { get { return ManagedListViewPanel1.AllowItemsDragAndDrop; } set { ManagedListViewPanel1.AllowItemsDragAndDrop = value; } }
        /// <summary>
        /// Allow columns reorder ? after a column reordered, the index of that column within the columns collection get changed
        /// </summary>
        [Description("Allow columns reorder ? after a column reordered, the index of that column within the columns collection get changed"), Category("ManagedListView")]
        public bool AllowColumnsReorder
        { get { return ManagedListViewPanel1.AllowColumnsReorder; } set { ManagedListViewPanel1.AllowColumnsReorder = value; } }
        /// <summary>
        /// If enabled, the sort mode of a column get changed when the user clicks that column
        /// </summary>
        [Description("If enabled, the sort mode of a column get changed when the user clicks that column"), Category("ManagedListView")]
        public bool ChangeColumnSortModeWhenClick
        { get { return ManagedListViewPanel1.ChangeColumnSortModeWhenClick; } set { ManagedListViewPanel1.ChangeColumnSortModeWhenClick = value; } }
        /// <summary>
        /// The thunmbnail height. Work only for thunmbnails view mode.
        /// </summary>
        [Description("The thunmbnail height. Work only for thumbnails view mode."), Category("ManagedListView")]
        public int ThunmbnailsHeight
        { get { return ManagedListViewPanel1.ThumbnailsHeight; } set { ManagedListViewPanel1.ThumbnailsHeight = value; ManagedListViewPanel1.Invalidate(); } }
        /// <summary>
        /// The thunmbnail width. Work only for thumbnails view mode.
        /// </summary>
        [Description("The thunmbnail width. Work only for thumbnails view mode."), Category("ManagedListView")]
        public int ThunmbnailsWidth
        { get { return ManagedListViewPanel1.ThumbnailsWidth; } set { ManagedListViewPanel1.ThumbnailsWidth = value; ManagedListViewPanel1.Invalidate(); } }
        /// <summary>
        /// The speed of the scroll when using mouse wheel. Default value is 20.
        /// </summary>
        [Description("The speed of the scroll when using mouse wheel. Default value is 20."), Category("ManagedListView")]
        public int WheelScrollSpeed
        { get { return wheelScrollSpeed; } set { wheelScrollSpeed = value; } }
        /// <summary>
        /// If enabled, the item get highlighted when the mouse over it
        /// </summary>
        [Description("If enabled, the item get highlighted when the mouse over it"), Category("ManagedListView")]
        public bool DrawHighlight
        { get { return ManagedListViewPanel1.DrawHighlight; } set { ManagedListViewPanel1.DrawHighlight = value; } }
        /// <summary>
        /// The images list that will be used for draw
        /// </summary>
        [Description("The images list that will be used for draw"), Category("ManagedListView")]
        public ImageList ImagesList
        { get { return ManagedListViewPanel1.ImagesList; } set { ManagedListViewPanel1.ImagesList = value; } }
        /// <summary>
        /// Get the selected items collection
        /// </summary>
        [Browsable(false)]
        public List<ManagedListViewItem> SelectedItems
        {
            get
            {
                return ManagedListViewPanel1.SelectedItems;
            }
        }
        /// <summary>
        /// Get or set if this control can accept dropped data
        /// </summary>
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                ManagedListViewPanel1.AllowDrop = value;
                base.AllowDrop = value;
            }
        }
        /// <summary>
        /// Get or set the font of this control
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
                ManagedListViewPanel1.Font = value;
            }
        }
        #endregion

        #region events
        /// <summary>
        /// Rised when the control need to draw a column
        /// </summary>
        [Description("Rised when the control need to draw a column. The column information will be sent along with this event args"), Category("ManagedListView")]
        public event EventHandler<ManagedListViewColumnDrawArgs> DrawColumn;
        /// <summary>
        /// Rised when the control need to draw an item. The item information will be sent along with this event args. Please note that this event rised only with Thumbnails View Mode.
        /// </summary>
        [Description("Rised when the control need to draw an item. The item information will be sent along with this event args. Please note that this event rised only with Thumbnails View Mode."), Category("ManagedListView")]
        public event EventHandler<ManagedListViewItemDrawArgs> DrawItem;
        /// <summary>
        /// Rised when the control need to draw a sub item
        /// </summary>
        [Description("Rised when the control need to draw a sub item. The sub item information will be sent along with this event args. NOTE: rised only if the sub item draw mode property equal None."), Category("ManagedListView")]
        public event EventHandler<ManagedListViewSubItemDrawArgs> DrawSubItem;
        /// <summary>
        /// Rised when the mouse is over a sub item
        /// </summary>
        [Description("Rised when the mouse get over a sub item."), Category("ManagedListView")]
        public event EventHandler<ManagedListViewMouseOverSubItemArgs> MouseOverSubItem;
        /// <summary>
        /// Rised when the item selection changed
        /// </summary>
        [Description("Rised when the user select/unselect items."), Category("ManagedListView")]
        public event EventHandler SelectedIndexChanged;
        /// <summary>
        /// Rised when the user clicks a column
        /// </summary>
        [Description("Rised when the user click on column."), Category("ManagedListView")]
        public event EventHandler<ManagedListViewColumnClickArgs> ColumnClicked;
        /// <summary>
        /// Rised when the user pressed the return key
        /// </summary>
        [Description("Rised when the user pressed the return key."), Category("ManagedListView")]
        public event EventHandler EnterPressed;
        /// <summary>
        /// Rised when the user double click on item
        /// </summary>
        [Description("Rised when the user double click on item"), Category("ManagedListView")]
        public event EventHandler<ManagedListViewItemDoubleClickArgs> ItemDoubleClick;
        /// <summary>
        /// Rised when the control needs to shwitch to the columns context menu
        /// </summary>
        [Description("Rised when the control needs to shwitch to the columns context menu"), Category("ManagedListView")]
        public event EventHandler SwitchToColumnsContextMenu;
        /// <summary>
        /// Rised when the control needs to shwitch to the normal context menu
        /// </summary>
        [Description("Rised when the control needs to shwitch to the normal context menu"), Category("ManagedListView")]
        public event EventHandler SwitchToNormalContextMenu;
        /// <summary>
        /// Rised when the user finished resizing a column
        /// </summary>
        [Description("Rised when the user finished resizing a column"), Category("ManagedListView")]
        public event EventHandler AfterColumnResize;
        /// <summary>
        /// Rised when the user draged item(s)
        /// </summary>
        [Description("Rised when the user draged item(s)"), Category("ManagedListView")]
        public event EventHandler ItemsDrag;
        /// <summary>
        /// Rised when the user changed the view mode
        /// </summary>
        [Description("Rised when the user changed the view mode"), Category("ManagedListView")]
        public event EventHandler ViewModeChanged;
        #endregion 

        #region Methods
        /// <summary>
        /// Retrieve item at current cursor point
        /// </summary>
        /// <returns>The found item</returns>
        public ManagedListViewItem GetItemAtCursorPoint()
        {
            return ManagedListViewPanel1.items[ManagedListViewPanel1.GetItemIndexAtCursorPoint()];
        }
        /// <summary>
        /// Retrieve item at point
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>The found item</returns>
        public ManagedListViewItem GetItemAtPoint(Point point)
        {
            return ManagedListViewPanel1.items[ManagedListViewPanel1.GetItemIndexAtPoint(point)];
        }
        /// <summary>
        /// Retrieve item index at current cursor point
        /// </summary>
        /// <returns>The found item index</returns>
        public int GetItemIndexAtCursorPoint()
        { return ManagedListViewPanel1.GetItemIndexAtCursorPoint(); }
        /// <summary>
        /// Retrieve item index at point
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>The found item index</returns>
        public int GetItemIndexAtPoint(Point point)
        { return ManagedListViewPanel1.GetItemIndexAtPoint(point); }
        /// <summary>
        /// Scroll view port into item
        /// </summary>
        /// <param name="itemIndex">The item index</param>
        public void ScrollToItem(int itemIndex)
        {
            try
            {
                ManagedListView_Resize(this, null);
                vScrollBar1.Value = ManagedListViewPanel1.GetVscrollValueForItem(itemIndex);
                ManagedListViewPanel1.VscrollOffset = ManagedListViewPanel1.GetVscrollValueForItem(itemIndex);
            }
            catch { }
        }
        /// <summary>
        /// Scroll view port into item
        /// </summary>
        /// <param name="item">The item to scroll into</param>
        public void ScrollToItem(ManagedListViewItem item)
        {
            ScrollToItem(ManagedListViewPanel1.items.IndexOf(item));
        }
        /// <summary>
        /// Rises the font changed event
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/></param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ManagedListViewPanel1.Font = Font;
        }
        #endregion

        private void ManagedListView_Paint(object sender, PaintEventArgs e)
        {
            ManagedListViewPanel1.Invalidate();
            ManagedListView_Resize(sender, null);
        }
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            ManagedListViewPanel1.HscrollOffset = hScrollBar1.Value;
            ManagedListViewPanel1.Invalidate();
        }
        private void ManagedListViewPanel1_RefreshValues(object sender, EventArgs e)
        {
            ManagedListView_Resize(sender, e);
        }
        private void ManagedListView_Resize(object sender, EventArgs e)
        {
            if (ManagedListViewPanel1.viewMode == ManagedListViewViewMode.Details)
            {
                var size = ManagedListViewPanel1.CalculateColumnsSize();
                if (size < Width)
                {
                    hScrollBar1.Maximum = 1;
                    ManagedListViewPanel1.HscrollOffset = hScrollBar1.Value = 0;
                    ManagedListViewPanel1.Invalidate();
                    hScrollBar1.Visible = false;
                }
                else
                {
                    hScrollBar1.Maximum = size - ManagedListViewPanel1.Width + 20;
                    hScrollBar1.Visible = true;
                }

                size = ManagedListViewPanel1.CalculateItemsSize();
                if (size < Height - 18)
                {
                    vScrollBar1.Maximum = 1;
                    ManagedListViewPanel1.VscrollOffset = vScrollBar1.Value = 0;
                    ManagedListViewPanel1.Invalidate();
                    vScrollBar1.Visible = false;
                }
                else
                {
                    vScrollBar1.Maximum = size - ManagedListViewPanel1.Height + 40;
                    vScrollBar1.Visible = true;
                }
            }
            else
            {
                var size = ManagedListViewPanel1.CalculateSizeOfItemsAsThumbnails().Height;
                if (size < Height - 18)
                {
                    vScrollBar1.Maximum = 1;
                    ManagedListViewPanel1.VscrollOffset = vScrollBar1.Value = 0;
                    ManagedListViewPanel1.Invalidate();
                    vScrollBar1.Visible = false;
                }
                else
                {
                    vScrollBar1.Maximum = size - ManagedListViewPanel1.Height + 40;
                    vScrollBar1.Visible = true;
                }

                size = ManagedListViewPanel1.CalculateSizeOfItemsAsThumbnails().Width;
                if (size < Width)
                {
                    hScrollBar1.Maximum = 1;
                    ManagedListViewPanel1.HscrollOffset = hScrollBar1.Value = 0;
                    ManagedListViewPanel1.Invalidate();
                    hScrollBar1.Visible = false;
                }
                else
                {
                    hScrollBar1.Maximum = size - ManagedListViewPanel1.Width + 20;
                    hScrollBar1.Visible = true;
                }
            }
        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            ManagedListViewPanel1.VscrollOffset = vScrollBar1.Value;
            ManagedListViewPanel1.Invalidate();
        }

        private void ManagedListViewPanel1_AdvanceVScrollRequest(object sender, EventArgs e)
        {
            try
            {
                vScrollBar1.Value += wheelScrollSpeed;
                ManagedListViewPanel1.VscrollOffset += wheelScrollSpeed;
            }
            catch { }
        }
        private void ManagedListViewPanel1_ReverseVScrollRequest(object sender, EventArgs e)
        {
            try
            {
                vScrollBar1.Value -= wheelScrollSpeed;
                ManagedListViewPanel1.VscrollOffset -= wheelScrollSpeed;
            }
            catch { }
        }
        private void ManagedListViewPanel1_RefreshScrollBars(object sender, EventArgs e)
        {
            ManagedListView_Resize(sender, e);
        }
        private void ManagedListViewPanel1_DrawColumn(object sender, ManagedListViewColumnDrawArgs e)
        {
            if (DrawColumn != null)
                DrawColumn(this, e);
        }
        private void ManagedListViewPanel1_DrawSubItem(object sender, ManagedListViewSubItemDrawArgs e)
        {
            if (DrawSubItem != null)
                DrawSubItem(this, e);
        }
        private void ManagedListViewPanel1_MouseOverSubItem(object sender, ManagedListViewMouseOverSubItemArgs e)
        {
            if (MouseOverSubItem != null)
                MouseOverSubItem(this, e);
        }
        private void ManagedListViewPanel1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, new EventArgs());
        }
        private void ManagedListViewPanel1_ColumnClicked(object sender, ManagedListViewColumnClickArgs e)
        {
            if (ColumnClicked != null)
                ColumnClicked(this, e);
        }
        private void ManagedListViewPanel1_DrawItem(object sender, ManagedListViewItemDrawArgs e)
        {
            if (DrawItem != null)
                DrawItem(this, e);
        }
       
        private void ManagedListViewPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }
        private void ManagedListViewPanel1_ClearScrolls(object sender, EventArgs e)
        {
            hScrollBar1.Maximum = 1;
            ManagedListViewPanel1.HscrollOffset = hScrollBar1.Value = 0;
            hScrollBar1.Enabled = false;

            vScrollBar1.Maximum = 1;
            ManagedListViewPanel1.VscrollOffset = vScrollBar1.Value = 0;
            vScrollBar1.Enabled = false;

            ManagedListViewPanel1.Invalidate();
        }
        private void hScrollBar1_KeyDown(object sender, KeyEventArgs e)
        {
            ManagedListViewPanel1.OnKeyDownRised(e);
        }
        private void ManagedListViewPanel1_ItemDoubleClick(object sender, ManagedListViewItemDoubleClickArgs e)
        {
            if (ItemDoubleClick != null)
                ItemDoubleClick(this, e);
        }
        private void ManagedListViewPanel1_EnterPressedOverItem(object sender, EventArgs e)
        {
            if (EnterPressed != null)
                EnterPressed(this, new EventArgs());
        }
   
        private void ManagedListViewPanel1_SwitchToColumnsContextMenu(object sender, EventArgs e)
        {
            if (SwitchToColumnsContextMenu != null)
                SwitchToColumnsContextMenu(this, new EventArgs());
        }
        private void ManagedListViewPanel1_SwitchToNormalContextMenu(object sender, EventArgs e)
        {
            if (SwitchToNormalContextMenu != null)
                SwitchToNormalContextMenu(this, new EventArgs());
        }
        private void ManagedListViewPanel1_AfterColumnResize(object sender, EventArgs e)
        {
            if (AfterColumnResize != null)
                AfterColumnResize(this, new EventArgs());
        }
        private void ManagedListViewPanel1_ItemsDrag(object sender, EventArgs e)
        {
            if (ItemsDrag != null)
                ItemsDrag(this, new EventArgs());
        }
     
        private void ManagedListViewPanel1_DragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }
        private void ManagedListViewPanel1_DragEnter(object sender, DragEventArgs e)
        {
            OnDragEnter(e);
        }
        private void ManagedListViewPanel1_DragLeave(object sender, EventArgs e)
        {
            OnDragLeave(e);
        }
        private void ManagedListViewPanel1_DragOver(object sender, DragEventArgs e)
        {
            OnDragOver(e);
        }
        private void ManagedListViewPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }
        private void ManagedListView_MouseLeave(object sender, EventArgs e)
        {
            ManagedListViewPanel1.OnMouseLeaveRise();
        }
        private void ManagedListView_MouseEnter(object sender, EventArgs e)
        {
            ManagedListView_Resize(sender, e);
        }
        private void ManagedListViewPanel1_KeyDown(object sender, KeyEventArgs e)
        {
            ManagedListViewPanel1.OnKeyDownRised(e);
        }
        private void ManagedListViewPanel1_ScrollToSelectedItemRequest(object sender, ManagedListViewItemSelectArgs e)
        {
            ScrollToItem(e.ItemIndex);
        }
    }
}
