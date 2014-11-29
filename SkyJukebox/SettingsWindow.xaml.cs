using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SkyJukebox.Core.Xml;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public static Settings Settings
        {
            get { return Settings.Instance; }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.SaveToXml();
        }
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            Settings.DiscardChanges();
        }
    }
}
