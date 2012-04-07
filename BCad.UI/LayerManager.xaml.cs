using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Data;
using BCad.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    public partial class LayerManager : BCadControl
    {
        private IWorkspace workspace;

        private ObservableCollection<MutableLayer> layers = new ObservableCollection<MutableLayer>();
        private ObservableCollection<Color> availableColors = new ObservableCollection<Color>();

        public ObservableCollection<MutableLayer> Layers
        {
            get { return this.layers; }
        }

        public ObservableCollection<Color> AvailableColors
        {
            get { return this.availableColors; }
        }

        [Obsolete("Default constructor is for WPF designer only")]
        public LayerManager()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeComponent();
            }
            else
            {
                throw new Exception("Default constructor is for WPF designer only");
            }
        }

        public LayerManager(IWorkspace workspace)
        {
            this.workspace = workspace;

            for (byte i = 0; i <= 9; i++)
                availableColors.Add(new Color(i));

            InitializeComponent();
        }

        public override void Initialize()
        {
            this.layers.Clear();
            foreach (var layer in workspace.Document.Layers.Values.OrderBy(l => l.Name))
            {
                this.layers.Add(new MutableLayer(layer));
            }
        }

        public override void Commit()
        {
            var doc = workspace.Document;

            if (this.layers.Where(layer => layer.IsDirty).Any())
            {
                // found changes, need to update
                var newLayers = new Dictionary<string, Layer>();
                foreach (var layer in from layer in this.layers
                                      select layer.GetUpdatedLayer())
                {
                    newLayers.Add(layer.Name, layer);
                }

                doc = doc.Update(layers: newLayers);
                workspace.Document = doc;
            }
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.layers.Add(new MutableLayer(
                StringUtilities.NextUniqueName("NewLayer", workspace.Document.Layers.Keys), Color.Auto));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayer;
            if (removed != null)
            {
                if (this.layers.Count == 1)
                    Debug.Fail("Cannot remove the last layer");

                if (!this.layers.Remove(removed))
                {
                    Debug.Fail("Layer could not be found");
                }
            }
        }
    }

    public class MutableLayer : DependencyObject
    {
        public MutableLayer(Layer layer)
        {
            this.DrawingLayer = layer;
            this.Name = layer.Name;
            this.Color = layer.Color;
        }

        public MutableLayer(string name, Color color)
        {
            this.DrawingLayer = null;
            this.Name = name;
            this.Color = color;
        }

        public Layer DrawingLayer { get; private set; }

        public bool IsDirty
        {
            get
            {
                return this.DrawingLayer == null
                    ? true
                    : this.Name != this.DrawingLayer.Name || this.Color != this.DrawingLayer.Color;
            }
        }

        public Layer GetUpdatedLayer()
        {
            if (this.DrawingLayer == null)
            {
                return new Layer(this.Name, this.Color);
            }
            else if (this.IsDirty)
            {
                return this.DrawingLayer.Update(name: this.Name, color: this.Color);
            }
            else
            {
                return this.DrawingLayer;
            }
        }

        public string Name
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(MutableLayer), new UIPropertyMetadata(string.Empty));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(MutableLayer), new UIPropertyMetadata(default(Color)));
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            bool param = bool.Parse(parameter as string);
            bool val = (bool)value;

            return val == param ?
              Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
