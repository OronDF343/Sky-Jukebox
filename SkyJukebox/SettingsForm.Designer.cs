namespace SkyJukebox
{
    partial class SettingsForm
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
            this.outputDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.registerButton = new System.Windows.Forms.Button();
            this.unregisterButton = new System.Windows.Forms.Button();
            this.shellMenuTextBox = new System.Windows.Forms.TextBox();
            this.restartAdminButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.volumeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.volumeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // outputDeviceComboBox
            // 
            this.outputDeviceComboBox.FormattingEnabled = true;
            this.outputDeviceComboBox.Location = new System.Drawing.Point(204, 12);
            this.outputDeviceComboBox.Name = "outputDeviceComboBox";
            this.outputDeviceComboBox.Size = new System.Drawing.Size(121, 21);
            this.outputDeviceComboBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Output Device:";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(16, 280);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(250, 280);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Exit";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 91);
            this.label2.TabIndex = 4;
            this.label2.Text = "Shell menu extension:\r\n\r\nWARNING: Very WIP.\r\nDo not use unless you\r\nknow what you" +
    " are doing! \r\nRequires Sky Jukebox to\r\nbe run as an administrator.";
            // 
            // registerButton
            // 
            this.registerButton.Location = new System.Drawing.Point(157, 123);
            this.registerButton.Name = "registerButton";
            this.registerButton.Size = new System.Drawing.Size(75, 23);
            this.registerButton.TabIndex = 5;
            this.registerButton.Text = "Register";
            this.registerButton.UseVisualStyleBackColor = true;
            this.registerButton.Click += new System.EventHandler(this.registerButton_Click);
            // 
            // unregisterButton
            // 
            this.unregisterButton.Location = new System.Drawing.Point(250, 123);
            this.unregisterButton.Name = "unregisterButton";
            this.unregisterButton.Size = new System.Drawing.Size(75, 23);
            this.unregisterButton.TabIndex = 6;
            this.unregisterButton.Text = "Unregister";
            this.unregisterButton.UseVisualStyleBackColor = true;
            this.unregisterButton.Click += new System.EventHandler(this.unregisterButton_Click);
            // 
            // shellMenuTextBox
            // 
            this.shellMenuTextBox.Location = new System.Drawing.Point(157, 97);
            this.shellMenuTextBox.Name = "shellMenuTextBox";
            this.shellMenuTextBox.Size = new System.Drawing.Size(168, 20);
            this.shellMenuTextBox.TabIndex = 7;
            this.shellMenuTextBox.Text = "Play with Sky Jukebox";
            // 
            // restartAdminButton
            // 
            this.restartAdminButton.Location = new System.Drawing.Point(157, 168);
            this.restartAdminButton.Name = "restartAdminButton";
            this.restartAdminButton.Size = new System.Drawing.Size(168, 23);
            this.restartAdminButton.TabIndex = 8;
            this.restartAdminButton.Text = "Restart Sky Jukebox as admin";
            this.restartAdminButton.UseVisualStyleBackColor = true;
            this.restartAdminButton.Click += new System.EventHandler(this.restartAdminButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Changing this setting will stop playback.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Volume:";
            // 
            // volumeNumericUpDown
            // 
            this.volumeNumericUpDown.Location = new System.Drawing.Point(64, 60);
            this.volumeNumericUpDown.Name = "volumeNumericUpDown";
            this.volumeNumericUpDown.Size = new System.Drawing.Size(81, 20);
            this.volumeNumericUpDown.TabIndex = 11;
            this.volumeNumericUpDown.ValueChanged += new System.EventHandler(this.volumeNumericUpDown_ValueChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 315);
            this.Controls.Add(this.volumeNumericUpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.restartAdminButton);
            this.Controls.Add(this.shellMenuTextBox);
            this.Controls.Add(this.unregisterButton);
            this.Controls.Add(this.registerButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.outputDeviceComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Sky Jukebox Settings";
            ((System.ComponentModel.ISupportInitialize)(this.volumeNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox outputDeviceComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button registerButton;
        private System.Windows.Forms.Button unregisterButton;
        private System.Windows.Forms.TextBox shellMenuTextBox;
        private System.Windows.Forms.Button restartAdminButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown volumeNumericUpDown;
    }
}