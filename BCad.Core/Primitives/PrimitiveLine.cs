using BCad.Helpers;
using System;

namespace BCad.Primitives
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

        public PrimitiveLine(Point p1, Point p2)
            : this(p1, p2, Color.Auto)
        {
        }

        public double Slope
        {
            get
            {
                var denom = P2.X - P1.X;
                return denom == 0.0 ? double.NaN : (P2.Y - P1.Y) / denom;
            }
        }

        public bool IntersectsInXY(PrimitiveLine other)
        {
            var m1 = this.Slope;
            var m2 = other.Slope;
            var b1 = this.P1.Y - m1 * this.P1.X;
            var b2 = other.P1.Y - m2 * other.P1.X;
            double x, y;

            // first line is vertical
            if (double.IsNaN(m1))
            {
                if (double.IsNaN(m2))
                    return false; // second line is vertical
                // if y-intersection is within first line's values they intersect
                x = this.P1.X;
                y = m2 * x + b2;
                return MathHelper.Between(Math.Min(this.P1.Y, this.P2.Y), Math.Max(this.P1.Y, this.P2.Y), y)
                    && MathHelper.Between(Math.Min(other.P1.X, other.P2.X), Math.Max(other.P1.X, other.P2.X), x);
            }
            else
            {
                // first line is not vertical
                if (double.IsNaN(m2)) // second line is vertical
                {
                    x = other.P1.X;
                    y = m1 * other.P1.X + b1;
                    return MathHelper.Between(Math.Min(other.P1.Y, other.P2.Y), Math.Max(other.P1.Y, other.P2.Y), y)
                        && MathHelper.Between(Math.Min(this.P1.X, this.P2.X), Math.Max(this.P1.X, this.P2.X), x);
                }
                else
                {
                    if (m1 == m2)
                        return false; // parallel
                    x = (b2 - b1) / (m1 - m2);
                    y = m1 * x + b1;
                    return MathHelper.Between(Math.Min(this.P1.Y, this.P2.Y), Math.Max(this.P1.Y, this.P2.Y), y)
                        && MathHelper.Between(Math.Min(other.P1.Y, other.P2.Y), Math.Max(other.P1.Y, other.P2.Y), y)
                        && MathHelper.Between(Math.Min(this.P1.X, this.P2.X), Math.Max(this.P1.X, this.P2.X), x)
                        && MathHelper.Between(Math.Min(other.P1.X, other.P2.X), Math.Max(other.P1.X, other.P2.X), x);
                }
            }
        }
    }
}
