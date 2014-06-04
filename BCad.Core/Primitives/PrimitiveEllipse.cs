using System.Diagnostics;
using BCad.Extensions;
using BCad.Helpers;

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
        public IndexedColor Color { get; private set; }
        public Matrix4 FromUnitCircle { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Ellipse; } }

        /// <summary>
        /// Creates a new PrimitiveEllipse.
        /// </summary>
        public PrimitiveEllipse(Point center, Vector majorAxis, Vector normal, double minorAxisRatio, double startAngle, double endAngle, IndexedColor color)
        {
            Debug.Assert(MathHelper.Between(0.0, 360.0, startAngle));
            Debug.Assert(MathHelper.Between(0.0, 360.0, endAngle));
            this.Center = center;
            this.MajorAxis = majorAxis;
            this.Normal = normal;
            this.MinorAxisRatio = minorAxisRatio;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Color = color;

            var right = majorAxis.Normalize();
            var up = normal.Cross(right).Normalize();
            var radiusX = majorAxis.Length;
            this.FromUnitCircle = Matrix4.FromUnitCircleProjection(normal.Normalize(), right, up, center, radiusX, radiusX * minorAxisRatio, 1.0);
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on a circle.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, Vector normal)
            : this(center, radius, normal, IndexedColor.Auto)
        {
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on a circle.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, Vector normal, IndexedColor color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, 0.0, 360.0, color)
        {
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on an arc.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, double startAngle, double endAngle, Vector normal, IndexedColor color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, startAngle, endAngle, color)
        {
        }

        /// <summary>
        /// Creates a circle that passes through the three specified points.  Null if the points are co-linear
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="idealNormal">The ideal normal to normalize to if specified.</param>
        /// <returns>The resultant circle or null.</returns>
        public static PrimitiveEllipse ThreePointCircle(Point a, Point b, Point c, Optional<Vector> idealNormal = default(Optional<Vector>))
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

            var center = b1.IntersectionPoint(b2, false);
            if (center == null)
                return null;

            if (idealNormal.HasValue && idealNormal.Value == normal * -1.0)
                normal = idealNormal.Value;

            return new PrimitiveEllipse(center.Value, (a - center.Value).Length, normal, IndexedColor.Auto);
        }

        /// <summary>
        /// Creates an arc that passes through the three specified points where the first and last
        /// points are the start and end points.  Null if the points are co-linear.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="idealNormal">The ideal normal to normalize to if specified.</param>
        /// <returns>The resultant arc or null.</returns>
        public static PrimitiveEllipse ThreePointArc(Point a, Point b, Point c, Optional<Vector> idealNormal = default(Optional<Vector>))
        {
            var circle = ThreePointCircle(a, b, c, idealNormal);
            if (circle != null)
            {
                var toUnit = circle.FromUnitCircle;
                toUnit.Invert();
                var startAngle = toUnit.Transform((Vector)a).ToAngle();
                var midAngle = toUnit.Transform((Vector)b).ToAngle();
                var endAngle = toUnit.Transform((Vector)c).ToAngle();

                // ensure the midpoint is included in the absolute angle span
                double realStart = startAngle, realMid = midAngle, realEnd = endAngle;
                if (realStart > realEnd)
                    realStart -= 360.0;

                if (realMid < realStart)
                    realMid += 360;


                if (realMid < realStart || realMid > realEnd)
                {
                    var temp = startAngle;
                    startAngle = endAngle;
                    endAngle = temp;
                }

                circle.StartAngle = startAngle;
                circle.EndAngle = endAngle;

                return circle;
            }

            return null;
        }

        /// <summary>
        /// Creates a 2-dimensional ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="a">The major radius.</param>
        /// <param name="b">The minor radius.</param>
        /// <returns>The PrimitiveEllipse object.</returns>
        public static PrimitiveEllipse Ellipse2d(Point center, double a, double b)
        {
            return new PrimitiveEllipse(center, new Vector(a, 0.0, 0.0), Vector.ZAxis, b / a, 0, 360, IndexedColor.Auto);
        }
    }
}
