using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using SkyJukebox.CoreApi.Utils;
using SkyJukebox.CoreApi.Xml;
using SkyJukebox.Utils;

namespace SkyJukebox
{
    public partial class SettingsForm : Form
    {
        private const string Keyname = "SkyJukeboxPlay";

        public SettingsForm()
        {
            InitializeComponent();
            var dt = new DataTable("devices");
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("guid", typeof(Guid));
            outputDeviceComboBox.ValueMember = "guid";
            outputDeviceComboBox.DisplayMember = "name";
            outputDeviceComboBox.DataSource = dt;
            foreach (var d in AudioUtils.GetOutputDevicesInfo())
                dt.Rows.Add(d.Key, d.Value);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Instance.PlaybackDevice = (Guid)outputDeviceComboBox.SelectedValue;
            Close();
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            var p = Assembly.GetExecutingAssembly().Location;
            FileShellExtension.Register("*", Keyname, shellMenuTextBox.Text, "\"" + p + "\" \"%1\"");
        }

        private void unregisterButton_Click(object sender, EventArgs e)
        {
            FileShellExtension.Unregister("*", Keyname);
        }
    }
}
