using BCad.Helpers;
using System;

namespace BCad.Primitives
{
    public class PrimitiveLine : IPrimitive
    {
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public IndexedColor Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Line; } }

        public PrimitiveLine(Point p1, Point p2, IndexedColor color)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.Color = color;
        }

        public PrimitiveLine(Point p1, Point p2)
            : this(p1, p2, IndexedColor.Auto)
        {
        }

        public PrimitiveLine(Point p1, double slope)
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

            this.Color = IndexedColor.Auto;
        }
    }
}
