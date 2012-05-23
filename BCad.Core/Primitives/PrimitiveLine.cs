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

            this.Color = Color.Auto;
        }

        public double Slope
        {
            get
            {
                var denom = P2.X - P1.X;
                return denom == 0.0 ? double.NaN : (P2.Y - P1.Y) / denom;
            }
        }

        public double PerpendicularSlope
        {
            get
            {
                var slope = this.Slope;
                if (double.IsNaN(slope))
                    return 0.0;
                else if (slope == 0.0)
                    return double.NaN;
                else
                    return -1.0 / slope;
            }
        }

        public Point IntersectionXY(PrimitiveLine other, bool withinSegment = true)
        {
            if (this.P1.Z != this.P2.Z || other.P1.Z != other.P2.Z || this.P1.Z != other.P1.Z)
            {
                // not all in the same plane
                return null;
            }

            var m1 = this.Slope;
            var m2 = other.Slope;
            var b1 = this.P1.Y - m1 * this.P1.X;
            var b2 = other.P1.Y - m2 * other.P1.X;
            var z = this.P1.Z;
            Point result;

            if (double.IsNaN(m1))
            {
                // first line is vertical
                if (double.IsNaN(m2))
                {
                    // second line is vertical; parallel
                    result = null;
                }
                else
                {
                    // we know the x-value, solve for y in `other`
                    result = new Point(this.P1.X, m2 * this.P1.X + b2, z);
                }
            }
            else
            {
                // first line is not vertial
                if (double.IsNaN(m2))
                {
                    // second line is vertical
                    result = new Point(other.P1.X, m1 * other.P1.X + b1, z);
                }
                else if (m1 == m2)
                {
                    // lines are non-vertial and parallel
                    result = null;
                }
                else
                {
                    // some intersection exists
                    var x = (b2 - b1) / (m1 - m2);
                    var y = m1 * x + b1;
                    result = new Point(x, y, z);
                }
            }

            if (result != null && withinSegment)
            {
                if (!MathHelper.Between(Math.Min(this.P1.X, this.P2.X), Math.Max(this.P1.X, this.P2.X), result.X) ||
                    !MathHelper.Between(Math.Min(this.P1.Y, this.P2.Y), Math.Max(this.P1.Y, this.P2.Y), result.Y) ||
                    !MathHelper.Between(Math.Min(other.P1.X, other.P2.X), Math.Max(other.P1.X, other.P2.X), result.X) ||
                    !MathHelper.Between(Math.Min(other.P1.Y, other.P2.Y), Math.Max(other.P1.Y, other.P2.Y), result.Y))
                {
                    result = null;
                }
            }

            return result;
        }
    }
}
