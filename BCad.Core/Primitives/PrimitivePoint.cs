namespace BCad.Primitives
{
    public class PrimitivePoint : IPrimitive
    {
        public Point Location { get; private set; }
        public IndexedColor Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Point; } }

        public PrimitivePoint(Point location, IndexedColor color)
        {
            Location = location;
            Color = color;
        }
    }
}
