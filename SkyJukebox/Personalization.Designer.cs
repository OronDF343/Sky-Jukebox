namespace SkyJukebox
{
    partial class Personalization
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.recolorCheckBox = new System.Windows.Forms.CheckBox();
            this.skinComboBox = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.progressColorButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressAlphaNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.bgAlphaNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.bgColorButton = new System.Windows.Forms.Button();
            this.defaultBgButton = new System.Windows.Forms.Button();
            this.defaultProgressButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.progressAlphaNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bgAlphaNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // colorButton
            // 
            this.colorButton.Location = new System.Drawing.Point(179, 8);
            this.colorButton.Name = "colorButton";
            this.colorButton.Size = new System.Drawing.Size(101, 23);
            this.colorButton.TabIndex = 0;
            this.colorButton.Text = "Choose color...";
            this.colorButton.UseVisualStyleBackColor = true;
            this.colorButton.Click += new System.EventHandler(this.colorButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Skin:";
            // 
            // recolorCheckBox
            // 
            this.recolorCheckBox.AutoSize = true;
            this.recolorCheckBox.Location = new System.Drawing.Point(12, 12);
            this.recolorCheckBox.Name = "recolorCheckBox";
            this.recolorCheckBox.Size = new System.Drawing.Size(91, 17);
            this.recolorCheckBox.TabIndex = 2;
            this.recolorCheckBox.Text = "Recolor icons";
            this.recolorCheckBox.UseVisualStyleBackColor = true;
            this.recolorCheckBox.CheckedChanged += new System.EventHandler(this.recolorCheckBox_CheckedChanged);
            // 
            // skinComboBox
            // 
            this.skinComboBox.FormattingEnabled = true;
            this.skinComboBox.Location = new System.Drawing.Point(151, 89);
            this.skinComboBox.Name = "skinComboBox";
            this.skinComboBox.Size = new System.Drawing.Size(129, 21);
            this.skinComboBox.TabIndex = 3;
            this.skinComboBox.SelectedIndexChanged += new System.EventHandler(this.skinComboBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(12, 129);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(205, 129);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Discard";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // progressColorButton
            // 
            this.progressColorButton.Location = new System.Drawing.Point(12, 60);
            this.progressColorButton.Name = "progressColorButton";
            this.progressColorButton.Size = new System.Drawing.Size(118, 23);
            this.progressColorButton.TabIndex = 6;
            this.progressColorButton.Text = "Progress Color...";
            this.progressColorButton.UseVisualStyleBackColor = true;
            this.progressColorButton.Click += new System.EventHandler(this.progressColorButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Alpha:";
            // 
            // progressAlphaNumericUpDown
            // 
            this.progressAlphaNumericUpDown.Location = new System.Drawing.Point(179, 63);
            this.progressAlphaNumericUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.progressAlphaNumericUpDown.Name = "progressAlphaNumericUpDown";
            this.progressAlphaNumericUpDown.Size = new System.Drawing.Size(42, 20);
            this.progressAlphaNumericUpDown.TabIndex = 8;
            this.progressAlphaNumericUpDown.Value = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.progressAlphaNumericUpDown.ValueChanged += new System.EventHandler(this.progressAlphaNumericUpDown_ValueChanged);
            // 
            // bgAlphaNumericUpDown
            // 
            this.bgAlphaNumericUpDown.Location = new System.Drawing.Point(179, 37);
            this.bgAlphaNumericUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.bgAlphaNumericUpDown.Name = "bgAlphaNumericUpDown";
            this.bgAlphaNumericUpDown.Size = new System.Drawing.Size(42, 20);
            this.bgAlphaNumericUpDown.TabIndex = 11;
            this.bgAlphaNumericUpDown.Value = new decimal(new int[] {
            191,
            0,
            0,
            0});
            this.bgAlphaNumericUpDown.ValueChanged += new System.EventHandler(this.bgAlphaNumericUpDown_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Alpha:";
            // 
            // bgColorButton
            // 
            this.bgColorButton.Location = new System.Drawing.Point(12, 34);
            this.bgColorButton.Name = "bgColorButton";
            this.bgColorButton.Size = new System.Drawing.Size(118, 23);
            this.bgColorButton.TabIndex = 9;
            this.bgColorButton.Text = "Background Color...";
            this.bgColorButton.UseVisualStyleBackColor = true;
            this.bgColorButton.Click += new System.EventHandler(this.bgColorButton_Click);
            // 
            // defaultBgButton
            // 
            this.defaultBgButton.Location = new System.Drawing.Point(228, 34);
            this.defaultBgButton.Name = "defaultBgButton";
            this.defaultBgButton.Size = new System.Drawing.Size(52, 23);
            this.defaultBgButton.TabIndex = 12;
            this.defaultBgButton.Text = "Default";
            this.defaultBgButton.UseVisualStyleBackColor = true;
            this.defaultBgButton.Click += new System.EventHandler(this.defaultBgButton_Click);
            // 
            // defaultProgressButton
            // 
            this.defaultProgressButton.Location = new System.Drawing.Point(228, 60);
            this.defaultProgressButton.Name = "defaultProgressButton";
            this.defaultProgressButton.Size = new System.Drawing.Size(52, 23);
            this.defaultProgressButton.TabIndex = 13;
            this.defaultProgressButton.Text = "Default";
            this.defaultProgressButton.UseVisualStyleBackColor = true;
            this.defaultProgressButton.Click += new System.EventHandler(this.defaultProgressButton_Click);
            // 
            // Personalization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 166);
            this.Controls.Add(this.defaultProgressButton);
            this.Controls.Add(this.defaultBgButton);
            this.Controls.Add(this.bgAlphaNumericUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bgColorButton);
            this.Controls.Add(this.progressAlphaNumericUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.progressColorButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.skinComboBox);
            this.Controls.Add(this.recolorCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.colorButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Personalization";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Sky Jukebox Personalization";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Personalization_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.progressAlphaNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bgAlphaNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button colorButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox recolorCheckBox;
        private System.Windows.Forms.ComboBox skinComboBox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button progressColorButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown progressAlphaNumericUpDown;
        private System.Windows.Forms.NumericUpDown bgAlphaNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bgColorButton;
        private System.Windows.Forms.Button defaultBgButton;
        private System.Windows.Forms.Button defaultProgressButton;
    }
}