using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SkyJukebox.Api;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Utils;
using SkyJukebox.Core.Xml;
using SkyJukebox.Lib;
using SkyJukebox.Lib.Extensions;
using SkyJukebox.Lib.Icons;
using Color = System.Drawing.Color;

namespace SkyJukebox.Widgets
{
    /// <summary>
    /// Interaction logic for PluginsWidget.xaml
    /// </summary>
    public partial class PluginsWidget
    {
        public PluginsWidget()
        {
            DisableAeroGlass = (bool)SettingsManager.Instance["DisableAeroGlass"].Value;
            InitializeComponent();
        }

        public PluginsWidget(Window parentWindow, Control showNear, WidgetRelativePosition relativePosition,
            WidgetAlignment alignment, bool allowOverlap, bool autoPosition)
            : this()
        {
            Initialize(parentWindow, showNear, relativePosition, alignment, allowOverlap, autoPosition);
        }

        public static IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }

        public static Brush BgBrush
        {
            get { return new SolidColorBrush(((Color)SettingsManager.Instance["BgColor"].Value).ToWpfColor()); }
        }

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                MainGrid.Focus();
        }

        public void AddButton(ExtensionInfo<IPlugin> p)
        {
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38, GridUnitType.Pixel) });
            var im = new Image();
            var bindingPath = String.Format("IconManagerInstance[{0}].ImageSource", p.Attribute.Id);
            im.SetBinding(Image.SourceProperty, new Binding
            {
                ElementName = "Plugins",
                Path = new PropertyPath(bindingPath)
            });
            var bt = new Button
            {
                BorderThickness = new Thickness(0),
                ToolTip = p.Attribute.Id,
                Content = im,
                Template = (ControlTemplate) FindResource("BorderlessButtonControlTemplate")
            };
            bt.Click += (sender, args) =>
            {
                DoFocusChange();
                p.Instance.ShowGui();
            };
            Grid.SetRow(bt, MainGrid.RowDefinitions.Count - 1);
            MainGrid.Children.Add(bt);
            Height = MainGrid.RowDefinitions.Count * 38 + 16;
            UpdatePosition();
        }
    }
}
