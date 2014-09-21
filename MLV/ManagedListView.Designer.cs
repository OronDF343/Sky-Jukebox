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
    partial class ManagedListView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.ManagedListViewPanel1 = new MLV.ManagedListViewPanel();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(477, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 316);
            this.vScrollBar1.TabIndex = 2;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            this.vScrollBar1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.hScrollBar1_KeyDown);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 316);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(494, 17);
            this.hScrollBar1.SmallChange = 0;
            this.hScrollBar1.TabIndex = 1;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            this.hScrollBar1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.hScrollBar1_KeyDown);
            // 
            // ManagedListViewPanel1
            // 
            this.ManagedListViewPanel1.AllowDrop = true;
            this.ManagedListViewPanel1.BackColor = System.Drawing.Color.White;
            this.ManagedListViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ManagedListViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.ManagedListViewPanel1.Name = "ManagedListViewPanel1";
            this.ManagedListViewPanel1.Size = new System.Drawing.Size(477, 316);
            this.ManagedListViewPanel1.TabIndex = 0;
            this.ManagedListViewPanel1.Text = "ManagedListViewPanel1";
            this.ManagedListViewPanel1.RefreshValues += new System.EventHandler(this.ManagedListViewPanel1_RefreshValues);
            this.ManagedListViewPanel1.ClearScrolls += new System.EventHandler(this.ManagedListViewPanel1_ClearScrolls);
            this.ManagedListViewPanel1.AdvanceVScrollRequest += new System.EventHandler(this.ManagedListViewPanel1_AdvanceVScrollRequest);
            this.ManagedListViewPanel1.ReverseVScrollRequest += new System.EventHandler(this.ManagedListViewPanel1_ReverseVScrollRequest);
            this.ManagedListViewPanel1.RefreshScrollBars += new System.EventHandler(this.ManagedListViewPanel1_RefreshScrollBars);
            this.ManagedListViewPanel1.SelectedIndexChanged += new System.EventHandler(this.ManagedListViewPanel1_SelectedIndexChanged);
            this.ManagedListViewPanel1.DrawColumn += new System.EventHandler<MLV.ManagedListViewColumnDrawArgs>(this.ManagedListViewPanel1_DrawColumn);
            this.ManagedListViewPanel1.DrawItem += new System.EventHandler<MLV.ManagedListViewItemDrawArgs>(this.ManagedListViewPanel1_DrawItem);
            this.ManagedListViewPanel1.DrawSubItem += new System.EventHandler<MLV.ManagedListViewSubItemDrawArgs>(this.ManagedListViewPanel1_DrawSubItem);
            this.ManagedListViewPanel1.MouseOverSubItem += new System.EventHandler<MLV.ManagedListViewMouseOverSubItemArgs>(this.ManagedListViewPanel1_MouseOverSubItem);
            this.ManagedListViewPanel1.ColumnClicked += new System.EventHandler<MLV.ManagedListViewColumnClickArgs>(this.ManagedListViewPanel1_ColumnClicked);
            this.ManagedListViewPanel1.ItemDoubleClick += new System.EventHandler<MLV.ManagedListViewItemDoubleClickArgs>(this.ManagedListViewPanel1_ItemDoubleClick);
            this.ManagedListViewPanel1.EnterPressedOverItem += new System.EventHandler(this.ManagedListViewPanel1_EnterPressedOverItem);
            this.ManagedListViewPanel1.SwitchToColumnsContextMenu += new System.EventHandler(this.ManagedListViewPanel1_SwitchToColumnsContextMenu);
            this.ManagedListViewPanel1.SwitchToNormalContextMenu += new System.EventHandler(this.ManagedListViewPanel1_SwitchToNormalContextMenu);
            this.ManagedListViewPanel1.AfterColumnResize += new System.EventHandler(this.ManagedListViewPanel1_AfterColumnResize);
            this.ManagedListViewPanel1.ItemsDrag += new System.EventHandler(this.ManagedListViewPanel1_ItemsDrag);
            this.ManagedListViewPanel1.ScrollToSelectedItemRequest += new System.EventHandler<MLV.ManagedListViewItemSelectArgs>(this.ManagedListViewPanel1_ScrollToSelectedItemRequest);
            this.ManagedListViewPanel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.ManagedListViewPanel1_DragDrop);
            this.ManagedListViewPanel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.ManagedListViewPanel1_DragEnter);
            this.ManagedListViewPanel1.DragOver += new System.Windows.Forms.DragEventHandler(this.ManagedListViewPanel1_DragOver);
            this.ManagedListViewPanel1.DragLeave += new System.EventHandler(this.ManagedListViewPanel1_DragLeave);
            this.ManagedListViewPanel1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ManagedListViewPanel1_KeyDown);
            this.ManagedListViewPanel1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ManagedListViewPanel1_MouseDoubleClick);
            this.ManagedListViewPanel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ManagedListViewPanel1_MouseMove);
            // 
            // ManagedListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ManagedListViewPanel1);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.hScrollBar1);
            this.Name = "ManagedListView";
            this.Size = new System.Drawing.Size(494, 333);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ManagedListView_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ManagedListViewPanel1_KeyDown);
            this.MouseEnter += new System.EventHandler(this.ManagedListView_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ManagedListView_MouseLeave);
            this.Resize += new System.EventHandler(this.ManagedListView_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private ManagedListViewPanel ManagedListViewPanel1;
        private System.Windows.Forms.VScrollBar vScrollBar1;

    }
}
