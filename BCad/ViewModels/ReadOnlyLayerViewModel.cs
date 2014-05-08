namespace BCad.ViewModels
{
    public class ReadOnlyLayerViewModel
    {
        private ColorMap colorMap;
        public Layer Layer { get; private set; }

        public ReadOnlyLayerViewModel(Layer layer, ColorMap colorMap)
        {
            Layer = layer;
            this.colorMap = colorMap;
        }

        public string Name
        {
            get { return Layer.Name; }
        }

        public bool IsVisible
        {
            get { return Layer.IsVisible; }
        }

        public RealColor RealColor
        {
            get { return colorMap[Layer.Color]; }
        }

        public static bool operator ==(ReadOnlyLayerViewModel a, ReadOnlyLayerViewModel b)
        {
            if ((object)a != null)
                return a.Equals(b);
            return (object)b == null;
        }

        public static bool operator !=(ReadOnlyLayerViewModel a, ReadOnlyLayerViewModel b)
        {
            if ((object)a != null)
                return !a.Equals(b);
            return (object)b != null;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyLayerViewModel)
            {
                var rl = (ReadOnlyLayerViewModel)obj;
                return this.Layer == rl.Layer;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Layer.GetHashCode();
        }
    }
}
