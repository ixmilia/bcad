using BCad.Extensions;

namespace BCad.Primitives
{
    public class PrimitiveEllipse : IPrimitive
    {
        public Point Center { get; private set; }
        public Vector MajorAxis { get; private set; }
        public Vector Normal { get; private set; }
        public double MinorAxisRatio { get; private set; }
        public double StartAngle { get; private set; }
        public double EndAngle { get; private set; }
        public Color Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Ellipse; } }

        /// <summary>
        /// Creates a new PrimitiveEllipse.
        /// </summary>
        public PrimitiveEllipse(Point center, Vector majorAxis, Vector normal, double minorAxisRatio, double startAngle, double endAngle, Color color)
        {
            this.Center = center;
            this.MajorAxis = majorAxis;
            this.Normal = normal;
            this.MinorAxisRatio = minorAxisRatio;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Color = color;
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on a circle.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, Vector normal, Color color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, 0.0, 360.0, color)
        {
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on an arc.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, startAngle, endAngle, color)
        {
        }

        /// <summary>
        /// Creates a circle that passes through the three specified points.  Null if the points are co-linear
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>The resultant circle or null.</returns>
        public static PrimitiveEllipse ThreePointCircle(Point a, Point b, Point c)
        {
            var v1 = a - b;
            var v2 = c - b;

            var normal = v1.Cross(v2);

            if (normal.IsZeroVector)
                return null;

            normal = normal.Normalize();

            var m1 = v1.Cross(normal);
            var m2 = v2.Cross(normal);

            var b1a = (a + b) / 2.0;
            var b2a = (c + b) / 2.0;

            var b1 = new PrimitiveLine(b1a, b1a + m1);
            var b2 = new PrimitiveLine(b2a, b2a + m2);

            var center = b1.IntersectionXY(b2, false);
            if (center == null)
                return null;

            return new PrimitiveEllipse(center, (a - center).Length, normal, Color.Auto);
        }
    }
}
