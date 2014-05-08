namespace BCad.ViewModels
{
    public class ColorViewModel
    {
        public ColorViewModel(IndexedColor color, RealColor realColor)
        {
            this.Color = color;
            this.RealColor = realColor;
        }

        public IndexedColor Color { get; private set; }

        public RealColor RealColor { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as ColorViewModel;
            if (other != null)
            {
                return other.Color == this.Color;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }
    }
}
