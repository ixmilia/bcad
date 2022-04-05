namespace IxMilia.BCad.Primitives
{
    public class PrimitivePoint : IPrimitive
    {
        public Point Location { get; private set; }
        public CadColor? Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Point; } }

        public PrimitivePoint(Point location, CadColor? color = null)
        {
            Location = location;
            Color = color;
        }

        public PrimitivePoint Update(
            Optional<Point> location = default,
            Optional<CadColor?> color = default)
        {
            var newLocation = location.HasValue ? location.Value : Location;
            var newColor = color.HasValue ? color.Value : Color;

            if (newLocation == Location &&
                newColor == Color)
            {
                // no change
                return this;
            }

            return new PrimitivePoint(newLocation, newColor);
        }
    }
}
