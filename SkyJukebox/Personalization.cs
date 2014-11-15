using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Xml;

namespace SkyJukebox
{
    public partial class Personalization : Form
    {
        public Personalization()
        {
            InitializeComponent();
            _cdGui.Color = _lastSelectedGuiColor = Settings.Instance.GuiColor;
            _cdProgress.Color = _lastSelectedProgressColor = Settings.Instance.ProgressColor;
            _cdBg.Color = _lastSelectedBgColor = Settings.Instance.BgColor;
            progressAlphaNumericUpDown.Value = _lastSelectedProgressColor.A;
            bgAlphaNumericUpDown.Value = _lastSelectedBgColor.A;
            recolorCheckBox.Checked = Settings.Instance.EnableRecolor;
            skinComboBox.DataSource = SkinManager.Instance.SkinRegistry.Values.ToList();
            skinComboBox.DisplayMember = "Name";
        }

        readonly ColorDialog _cdGui = new ColorDialog { AnyColor = true, SolidColorOnly = true };
        readonly ColorDialog _cdProgress = new ColorDialog { AnyColor = true };
        readonly ColorDialog _cdBg = new ColorDialog { AnyColor = true };
        private Color _lastSelectedGuiColor, _lastSelectedProgressColor, _lastSelectedBgColor;
        private void colorButton_Click(object sender, EventArgs e)
        {
            if (_cdGui.ShowDialog() == DialogResult.OK && recolorCheckBox.Checked)
                InstanceManager.MiniPlayerInstance.SetIconColor(_lastSelectedGuiColor = _cdGui.Color);
        }

        private void recolorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (recolorCheckBox.Checked) InstanceManager.MiniPlayerInstance.SetIconColor(_lastSelectedGuiColor);
            else InstanceManager.MiniPlayerInstance.ResetIconColor();
        }

        private bool _saved = false;
        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Instance.EnableRecolor.Value = recolorCheckBox.Checked;
            Settings.Instance.GuiColor.Value = _lastSelectedGuiColor;
            Settings.Instance.SelectedSkin.Value = skinComboBox.SelectedText;
            Settings.Instance.ProgressColor.Value = _lastSelectedProgressColor;
            Settings.Instance.BgColor.Value = _lastSelectedBgColor;
            _saved = true;
            Close();
        }

        private void skinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skinComboBox.SelectedText != "")
                if (!IconManager.Instance.LoadFromSkin(skinComboBox.SelectedText))
                {
                    MessageBox.Show("Failed to load skin!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    skinComboBox.SelectedIndex = 0;
                }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void progressColorButton_Click(object sender, EventArgs e)
        {
            if (_cdProgress.ShowDialog() == DialogResult.OK)
                InstanceManager.MiniPlayerInstance.SetProgressColor(_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _cdProgress.Color.R, _cdProgress.Color.G, _cdProgress.Color.B));
        }

        private void progressAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.SetProgressColor(_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _lastSelectedProgressColor.R, _lastSelectedProgressColor.G, _lastSelectedProgressColor.B));
        }

        private void bgAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.SetBgColor(_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _lastSelectedBgColor.R, _lastSelectedBgColor.G, _lastSelectedBgColor.B));
        }

        private void bgColorButton_Click(object sender, EventArgs e)
        {
            if (_cdBg.ShowDialog() == DialogResult.OK)
                InstanceManager.MiniPlayerInstance.SetBgColor(_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _cdBg.Color.R, _cdBg.Color.G, _cdBg.Color.B));
        }

        private void Personalization_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cdBg.Dispose();
            _cdGui.Dispose();
            _cdProgress.Dispose();
            if (_saved) return;
            IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin);
            InstanceManager.MiniPlayerInstance.SetBgColor(Settings.Instance.BgColor);
            InstanceManager.MiniPlayerInstance.SetProgressColor(Settings.Instance.ProgressColor);
            if (Settings.Instance.EnableRecolor)
                InstanceManager.MiniPlayerInstance.SetIconColor(Settings.Instance.GuiColor);
            else
                InstanceManager.MiniPlayerInstance.ResetIconColor();
        }

        private void defaultBgButton_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.SetBgColor(_cdBg.Color = _lastSelectedBgColor = Settings.Instance.BgColor.DefaultValue);
            bgAlphaNumericUpDown.Value = _lastSelectedBgColor.A;
        }

        private void defaultProgressButton_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.SetProgressColor(_cdProgress.Color = _lastSelectedProgressColor = Settings.Instance.ProgressColor.DefaultValue);
            progressAlphaNumericUpDown.Value = _lastSelectedProgressColor.A;
        }
    }
}
