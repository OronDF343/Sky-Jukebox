using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace SkyJukebox
{
    public partial class SettingsForm : Form
    {
        private DataTable dt;
        public SettingsForm()
        {
            InitializeComponent();
            dt = new DataTable("devices");
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("guid", typeof(Guid));
            outputDeviceComboBox.ValueMember = "guid";
            outputDeviceComboBox.DisplayMember = "name";
            outputDeviceComboBox.DataSource = dt;
            foreach (var d in DirectSoundOut.Devices)
                dt.Rows.Add(d.Description, d.Guid);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Instance.Settings.PlaybackDevice = (Guid)outputDeviceComboBox.SelectedValue;
            Close();
        }
    }
}
