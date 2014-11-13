using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
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
                dt.Rows.Add(d.Value, d.Key);
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
            try
            {
                FileShellExtension.Register("*", Keyname, shellMenuTextBox.Text, "\"" + InstanceManager.ExeFilePath + "\" \"%1\"");
                FileShellExtension.Register("Directory", Keyname, shellMenuTextBox.Text, "\"" + InstanceManager.ExeFilePath + "\" \"%1\"");
            }
            catch
            {
                MessageBox.Show("Failed to register! Try restarting Sky Jukebox as an administrator.", "Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Asterisk);
            }
        }

        private void unregisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                FileShellExtension.Unregister("*", Keyname);
                FileShellExtension.Unregister("Directory", Keyname);
            }
            catch
            {
                MessageBox.Show("Failed to unregister! Try restarting Sky Jukebox as an administrator.", "Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Asterisk);
            }
        }

        private void restartAdminButton_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo(InstanceManager.ExeFilePath)
            {
                Arguments = "--wait",
                Verb = "runas"
            };
            Process.Start(startInfo);
            InstanceManager.MiniPlayerInstance.Close();
        }
    }
}
