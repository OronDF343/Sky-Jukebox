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
            recolorCheckBox.Checked = Settings.Instance.EnableRecolor;
            skinComboBox.DataSource = SkinManager.Instance.SkinRegistry.Values.ToList();
            skinComboBox.DisplayMember = "Name";
        }

        readonly ColorDialog _cd = new ColorDialog { AnyColor = true, SolidColorOnly = true };
        private Color _lastselectedColor;
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
            Close();
        }

        private void skinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
