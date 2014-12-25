using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using Color = System.Drawing.Color;

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

        private bool _saved;
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
            if (skinComboBox.SelectedText == "") return;
            if (IconManager.Instance.LoadFromSkin(skinComboBox.SelectedText)) return;
            MessageBox.Show("Failed to load skin!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            skinComboBox.SelectedIndex = 0;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void progressColorButton_Click(object sender, EventArgs e)
        {
            if (_cdProgress.ShowDialog() == DialogResult.OK)
                InstanceManager.MiniPlayerInstance.FilledColumnBrush = new SolidColorBrush((_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _cdProgress.Color.R, _cdProgress.Color.G, _cdProgress.Color.B)).ToWpfColor());
        }

        private void progressAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.FilledColumnBrush = new SolidColorBrush((_lastSelectedProgressColor = Color.FromArgb((int)progressAlphaNumericUpDown.Value, _lastSelectedProgressColor.R, _lastSelectedProgressColor.G, _lastSelectedProgressColor.B)).ToWpfColor());
        }

        private void bgAlphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.EmptyColumnBrush = new SolidColorBrush((_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _lastSelectedBgColor.R, _lastSelectedBgColor.G, _lastSelectedBgColor.B)).ToWpfColor());
        }

        private void bgColorButton_Click(object sender, EventArgs e)
        {
            if (_cdBg.ShowDialog() == DialogResult.OK)
                InstanceManager.MiniPlayerInstance.EmptyColumnBrush = new SolidColorBrush((_lastSelectedBgColor = Color.FromArgb((int)bgAlphaNumericUpDown.Value, _cdBg.Color.R, _cdBg.Color.G, _cdBg.Color.B)).ToWpfColor());
        }

        private void Personalization_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cdBg.Dispose();
            _cdGui.Dispose();
            _cdProgress.Dispose();
            if (_saved) return;
            IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin);
            InstanceManager.MiniPlayerInstance.EmptyColumnBrush = new SolidColorBrush(Settings.Instance.BgColor.Value.ToWpfColor());
            InstanceManager.MiniPlayerInstance.FilledColumnBrush = new SolidColorBrush(Settings.Instance.ProgressColor.Value.ToWpfColor());
            if (Settings.Instance.EnableRecolor)
                InstanceManager.MiniPlayerInstance.SetIconColor(Settings.Instance.GuiColor);
            else
                InstanceManager.MiniPlayerInstance.ResetIconColor();
        }

        private void defaultBgButton_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.EmptyColumnBrush = new SolidColorBrush((_cdBg.Color = _lastSelectedBgColor = Settings.Instance.BgColor.DefaultValue).ToWpfColor());
            bgAlphaNumericUpDown.Value = _lastSelectedBgColor.A;
        }

        private void defaultProgressButton_Click(object sender, EventArgs e)
        {
            InstanceManager.MiniPlayerInstance.FilledColumnBrush = new SolidColorBrush((_cdProgress.Color = _lastSelectedProgressColor = Settings.Instance.ProgressColor.DefaultValue).ToWpfColor());
            progressAlphaNumericUpDown.Value = _lastSelectedProgressColor.A;
        }
    }
}
