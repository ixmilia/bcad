namespace BCad.Entities
{
    public class PrimitiveLine : IPrimitive
    {
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public Color Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Line; } }

        public PrimitiveLine(Point p1, Point p2, Color color)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.Color = color;
        }
    }
}
