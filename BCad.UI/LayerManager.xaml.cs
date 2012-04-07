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
        private List<MutableLayer> addedLayers = new List<MutableLayer>();
        private List<MutableLayer> removedLayers = new List<MutableLayer>();

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
            var updatedLayers = new Dictionary<string, Layer>(doc.Layers);

            // update existing layers
            foreach (var layer in from l in this.layers
                                  where l.IsChanged
                                     && !this.addedLayers.Contains(l)
                                     && !this.removedLayers.Contains(l)
                                  select l)
            {
                updatedLayers[layer.Name] = layer.DrawingLayer.Update(name: layer.Name, color: layer.Color);
            }

            // remove deleted layers
            foreach (var layer in this.removedLayers)
                updatedLayers.Remove(layer.Name);

            // add new layers
            foreach (var layer in this.addedLayers)
                updatedLayers.Add(layer.Name, layer.DrawingLayer.Update(name: layer.Name, color: layer.Color));

            doc = doc.Update(layers: updatedLayers);
            workspace.Document = doc;
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var layer = new MutableLayer(
                new Layer(StringUtilities.NextUniqueName("NewLayer", workspace.Document.Layers.Keys), Color.Auto));
            this.layers.Add(layer);
            this.addedLayers.Add(layer);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayer;
            if (removed != null)
            {
                if (this.layers.Count == 1)
                    Debug.Fail("Cannot remove the last layer");

                if (!this.addedLayers.Remove(removed))
                {
                    if (!this.layers.Remove(removed))
                    {
                        Debug.Fail("Layer could not be found");
                    }
                    this.removedLayers.Add(removed);
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

        public Layer DrawingLayer { get; private set; }

        public bool IsChanged
        {
            get
            {
                return this.Name != this.Name || this.Color != this.Color;
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
