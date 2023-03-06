namespace IxMilia.BCad.Primitives
{
    public class PrimitiveLine : IPrimitive
    {
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public CadColor? Color { get; private set; }
        public double Thickness { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Line; } }

        public double Length => (P2 - P1).Length;
        public double LengthSquared => (P2 - P1).LengthSquared;

        public PrimitiveLine(Point p1, Point p2, CadColor? color = null, double thickness = default(double))
        {
            this.P1 = p1;
            this.P2 = p2;
            this.Color = color;
            this.Thickness = thickness;
        }

        public PrimitiveLine(Point p1, double slope, CadColor? color = null)
        {
            this.P1 = p1;
            if (double.IsNaN(slope))
            {
                // vertical
                this.P2 = new Point(p1.X, p1.Y + 1.0, p1.Z);
            }
            else
            {
                this.P2 = this.P1 + new Vector(1.0, slope, 0.0);
            }

            this.Color = color;
        }

        public override string ToString() => $"P1 = {P1}, P2 = {P2}";

        public PrimitiveLine Update(
            Optional<Point> p1 = default,
            Optional<Point> p2 = default,
            Optional<CadColor?> color = default,
            Optional<double> thickness = default)
        {
            var newP1 = p1.HasValue ? p1.Value : P1;
            var newP2 = p2.HasValue ? p2.Value : P2;
            var newColor = color.HasValue ? color.Value : Color;
            var newThickness = thickness.HasValue ? thickness.Value : Thickness;

            if (newP1 == P1 &&
                newP2 == P2 &&
                newColor == Color &&
                newThickness == Thickness)
            {
                // no change
                return this;
            }

            return new PrimitiveLine(newP1, newP2, newColor, newThickness);
        }
    }
}
