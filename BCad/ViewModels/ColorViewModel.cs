namespace BCad.ViewModels
{
    public class ColorViewModel
    {
        public ColorViewModel(CadColor? color)
        {
            this.Color = color;
        }

        public CadColor? Color { get; private set; }

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
