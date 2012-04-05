using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Data;

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
            foreach (var layer in this.layers)
            {
                doc = doc.Replace(layer.DrawingLayer, layer.DrawingLayer.Update(name: layer.Name, color: layer.Color));
            }

            // TODO: need to track added/deleted layers

            workspace.Document = doc;
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            DependencyProperty.Register("Color", typeof(Color), typeof(MutableLayer), new UIPropertyMetadata(Color.Auto));
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
