using System.Windows;

namespace BCad.UI.Controls
{
    public class MutableLayer : DependencyObject
    {
        public MutableLayer(Layer layer)
        {
            this.DrawingLayer = layer;
            this.Name = layer.Name;
            this.Color = layer.Color;
            this.IsVisible = layer.IsVisible;
        }

        public MutableLayer(string name, Color color)
        {
            this.DrawingLayer = null;
            this.Name = name;
            this.Color = color;
            this.IsVisible = true;
        }

        public Layer DrawingLayer { get; private set; }

        public bool IsDirty
        {
            get
            {
                return this.DrawingLayer == null
                    ? true
                    : this.Name != this.DrawingLayer.Name ||
                      this.Color != this.DrawingLayer.Color ||
                      this.IsVisible != this.DrawingLayer.IsVisible;
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
                return this.DrawingLayer.Update(name: this.Name, color: this.Color, isVisible: this.IsVisible ?? false);
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

        public bool? IsVisible
        {
            get { return (bool?)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool?), typeof(MutableLayer), new UIPropertyMetadata(null));
    }
}
