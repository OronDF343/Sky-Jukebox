using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SkyJukebox.Core.Icons;
using SkyJukebox.Core.Xml;

namespace SkyJukebox.Widgets
{
    /// <summary>
    /// Interaction logic for PluginsWidget.xaml
    /// </summary>
    public partial class PluginsWidget : INotifyPropertyChanged
    {
        public PluginsWidget()
        {
            DisableAeroGlass = (bool)SettingsManager.Instance["DisableAeroGlass"].Value;
            InitializeComponent();
            IconManagerInstance.CollectionChanged += (sender, args) => OnPropertyChanged("IconManagerInstance");
        }

        public PluginsWidget(Window parentWindow, Control showNear, WidgetRelativePosition relativePosition,
            WidgetAlignment alignment, bool allowOverlap, bool autoPosition)
            : this()
        {
            Initialize(parentWindow, showNear, relativePosition, alignment, allowOverlap, autoPosition);
        }

        public IconManager IconManagerInstance
        {
            get { return IconManager.Instance; }
        }

        public SettingsManager SettingsInstance { get { return SettingsManager.Instance; } }

        private void DoFocusChange()
        {
            if (Visibility == Visibility.Visible)
                MainGrid.Focus();
        }

        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>();

        public void AddButton(string btnId, string iconId, Action onClick, string toolTip)
        {
            if (_buttons.ContainsKey(btnId))
            {
                if (iconId != null)
                {
                    BindingOperations.ClearBinding(((Image)_buttons[btnId].Content), Image.SourceProperty);
                    var path = String.Format("IconManagerInstance[{0}].ImageSource", iconId);
                    ((Image)_buttons[btnId].Content).SetBinding(Image.SourceProperty, new Binding
                    {
                        ElementName = "Plugins",
                        Path = new PropertyPath(path)
                    });
                }
                if (toolTip != null)
                    _buttons[btnId].ToolTip = toolTip;
                return;
            }
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38, GridUnitType.Pixel) });
            var im = new Image();
            var bindingPath = String.Format("IconManagerInstance[{0}].ImageSource", iconId);
            im.SetBinding(Image.SourceProperty, new Binding
            {
                ElementName = "Plugins",
                Path = new PropertyPath(bindingPath)
            });
            var bt = new Button
            {
                BorderThickness = new Thickness(0),
                ToolTip = toolTip,
                Content = im,
                Template = (ControlTemplate)FindResource("BorderlessButtonControlTemplate")
            };
            bt.Click += (sender, args) =>
            {
                DoFocusChange();
                onClick();
            };
            Grid.SetRow(bt, MainGrid.RowDefinitions.Count - 1);
            _buttons.Add(btnId, bt);
            MainGrid.Children.Add(bt);
            Height = MainGrid.RowDefinitions.Count * 38 + 16;
            UpdatePosition();
        }

        public void RemoveButton(string btnId)
        {
            if (_buttons.ContainsKey(btnId))
                MainGrid.Children.Remove(_buttons[btnId]);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
