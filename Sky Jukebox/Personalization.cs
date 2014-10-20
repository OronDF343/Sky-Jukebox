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
            _cd.Color = _lastselectedColor = Settings.Instance.GuiColor;
            _cd2.Color = _lastselectedColor2 = Settings.Instance.ProgressColor;
            recolorCheckBox.Checked = Settings.Instance.EnableRecolor;
            skinComboBox.DataSource = SkinManager.Instance.SkinRegistry.Values.ToList();
            skinComboBox.DisplayMember = "Name";
        }

        readonly ColorDialog _cd = new ColorDialog { AnyColor = true, SolidColorOnly = true };
        readonly ColorDialog _cd2 = new ColorDialog { AnyColor = true };
        private Color _lastselectedColor;
        private Color _lastselectedColor2;
        private void colorButton_Click(object sender, EventArgs e)
        {
            if (_cd.ShowDialog() == DialogResult.OK && recolorCheckBox.Checked)
                Instance.MiniPlayerInstance.SetColor(_lastselectedColor = _cd.Color);
        }

        private void recolorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (recolorCheckBox.Checked) Instance.MiniPlayerInstance.SetColor(_lastselectedColor);
            else Instance.MiniPlayerInstance.ResetColor();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Instance.EnableRecolor.Value = recolorCheckBox.Checked;
            Settings.Instance.GuiColor = _lastselectedColor;
            Settings.Instance.SelectedSkin = skinComboBox.SelectedText;
            Settings.Instance.ProgressColor = _lastselectedColor2;
            Close();
        }

        private void skinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skinComboBox.SelectedText != "")
                IconManager.Instance.LoadFromSkin(skinComboBox.SelectedText);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            IconManager.Instance.LoadFromSkin(Settings.Instance.SelectedSkin ?? "Default Skin");
            Close();
        }

        private void progressColorButton_Click(object sender, EventArgs e)
        {
            if (_cd2.ShowDialog() == DialogResult.OK)
                Instance.MiniPlayerInstance.SetProgressColor(_lastselectedColor2 = Color.FromArgb((int)alphaNumericUpDown.Value, _cd2.Color.R, _cd2.Color.G, _cd2.Color.B));
        }

        private void alphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            Instance.MiniPlayerInstance.SetProgressColor(_lastselectedColor2 = Color.FromArgb((int)alphaNumericUpDown.Value, _lastselectedColor2.R, _lastselectedColor2.G, _lastselectedColor2.B));
        }
    }
}
