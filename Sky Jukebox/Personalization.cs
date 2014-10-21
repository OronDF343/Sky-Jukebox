using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyJukebox.Data;
using SkyJukebox.Icons;

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
                Instance.MiniPlayerInstance.SetIconColor(_lastSelectedGuiColor = _cdGui.Color);
        }

        private void recolorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (recolorCheckBox.Checked) Instance.MiniPlayerInstance.SetIconColor(_lastSelectedGuiColor);
            else Instance.MiniPlayerInstance.ResetIconColor();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Instance.EnableRecolor.Value = recolorCheckBox.Checked;
            Settings.Instance.GuiColor.Value = _lastSelectedGuiColor;
            Settings.Instance.SelectedSkin = skinComboBox.SelectedText;
            Settings.Instance.ProgressColor.Value = _lastSelectedProgressColor;
            Settings.Instance.BgColor.Value = _lastSelectedBgColor;
            Close();
        }

        private void skinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skinComboBox.SelectedText != "")
                IconManager.Instance.LoadFromSkin(skinComboBox.SelectedText);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void progressColorButton_Click(object sender, EventArgs e)
        {
            if (_cdProgress.ShowDialog() == DialogResult.OK)
                Instance.MiniPlayerInstance.SetProgressColor(_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _cdProgress.Color.R, _cdProgress.Color.G, _cdProgress.Color.B));
        }

        private void progressAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.SetProgressColor(_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _lastSelectedProgressColor.R, _lastSelectedProgressColor.G, _lastSelectedProgressColor.B));
        }

        private void bgAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.SetBgColor(_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _lastSelectedBgColor.R, _lastSelectedBgColor.G, _lastSelectedBgColor.B));
        }

        private void bgColorButton_Click(object sender, EventArgs e)
        {
            if (_cdBg.ShowDialog() == DialogResult.OK)
                Instance.MiniPlayerInstance.SetBgColor(_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _cdBg.Color.R, _cdBg.Color.G, _cdBg.Color.B));
        }

        private void Personalization_FormClosing(object sender, FormClosingEventArgs e)
        {
            IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin == "" ? "Default Skin" : Settings.Instance.SelectedSkin);
            Instance.MiniPlayerInstance.ResetBgColor();
            Instance.MiniPlayerInstance.ResetProgressColor();
            Instance.MiniPlayerInstance.ResetIconColor();
        }

        private void defaultBgButton_Click(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.SetBgColor(_cdBg.Color = _lastSelectedBgColor = Settings.Instance.BgColor.DefaultValue);
            bgAlphaNumericUpDown.Value = _lastSelectedBgColor.A;
        }

        private void defaultProgressButton_Click(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.SetProgressColor(_cdProgress.Color = _lastSelectedProgressColor = Settings.Instance.ProgressColor.DefaultValue);
            progressAlphaNumericUpDown.Value = _lastSelectedProgressColor.A;
        }
    }
}
