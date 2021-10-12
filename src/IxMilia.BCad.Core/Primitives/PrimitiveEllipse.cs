using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Primitives
{
    public class PrimitiveEllipse : IPrimitive
    {
        public const double BezierConstant = 0.551915024494;

        public Point Center { get; private set; }
        public Vector MajorAxis { get; private set; }
        public Vector Normal { get; private set; }
        public double MinorAxisRatio { get; private set; }
        public double StartAngle { get; private set; }
        public double EndAngle { get; private set; }
        public CadColor? Color { get; private set; }
        public Matrix4 FromUnitCircle { get; private set; }
        public double Thickness { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Ellipse; } }

        public bool IsClosed { get { return MathHelper.CloseTo(0.0, StartAngle) && MathHelper.CloseTo(MathHelper.ThreeSixty, EndAngle); } }

        public bool IsCircular => MathHelper.CloseTo(1.0, MinorAxisRatio);

        public bool IsCircle => IsCircular && IsClosed;

        /// <summary>
        /// Creates a new PrimitiveEllipse.
        /// </summary>
        public PrimitiveEllipse(Point center, Vector majorAxis, Vector normal, double minorAxisRatio, double startAngle, double endAngle, CadColor? color = null, double thickness = default(double))
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
            this.Thickness = thickness;

            var right = majorAxis.Normalize();
            var up = normal.Cross(right).Normalize();
            var radiusX = majorAxis.Length;
            this.FromUnitCircle = Matrix4.FromUnitCircleProjection(normal.Normalize(), right, up, center, radiusX, radiusX * minorAxisRatio, 1.0);
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on a circle.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, Vector normal, CadColor? color = null, double thickness = default(double))
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, 0.0, 360.0, color, thickness)
        {
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on an arc.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, double startAngle, double endAngle, Vector normal, CadColor? color = null, double thickness = default(double))
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, startAngle, endAngle, color, thickness)
        {
        }

        /// <summary>
        /// Returns a collection of 4 <see cref="PrimitiveBezier"/>s that represent the untrimmed ellipse.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PrimitiveBezier> AsBezierCurves()
        {
            var minorAxis = MajorAxis.Cross(Normal).Normalize() * MajorAxis.Length * MinorAxisRatio;
            var majorAxisScaled = MajorAxis.Normalize() * MajorAxis.Length * BezierConstant;
            var minorAxisScaled = minorAxis.Normalize() * minorAxis.Length * BezierConstant;

            // first quadrant
            yield return new PrimitiveBezier(
                Center + MajorAxis,
                Center + MajorAxis + minorAxisScaled,
                Center + minorAxis + majorAxisScaled,
                Center + minorAxis);

            // second quadrant
            yield return new PrimitiveBezier(
                Center + minorAxis,
                Center + minorAxis - majorAxisScaled,
                Center - MajorAxis + minorAxisScaled,
                Center - MajorAxis);

            // third quadrant
            yield return new PrimitiveBezier(
                Center - MajorAxis,
                Center - MajorAxis - minorAxisScaled,
                Center - minorAxis - majorAxisScaled,
                Center - minorAxis);

            // fourth quadrant
            yield return new PrimitiveBezier(
                Center - minorAxis,
                Center - minorAxis + majorAxisScaled,
                Center + MajorAxis - minorAxisScaled,
                Center + MajorAxis);
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

            return new PrimitiveEllipse(center.GetValueOrDefault(), (a - center.GetValueOrDefault()).Length, normal);
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
                var toUnit = circle.FromUnitCircle.Inverse();
                var startAngle = toUnit.Transform((Vector)a).ToAngle();
                var midAngle = toUnit.Transform((Vector)b).ToAngle();
                var endAngle = toUnit.Transform((Vector)c).ToAngle();

                // normalize mid and end angles to be greater than the start angle
                var realMid = midAngle;
                while (realMid < startAngle)
                    realMid += MathHelper.ThreeSixty;

                var realEnd = endAngle;
                while (realEnd < startAngle)
                    realEnd += MathHelper.ThreeSixty;

                if (realMid > realEnd)
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
        /// Creates an arc with the specified endpoints and the resultant included angle and vertex direction.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="includedAngle">The included angle of the resultant arc in degrees.</param>
        /// <param name="vertexDirection">The direction of the vertices specifying the arc.</param>
        /// <returns>The resultant arc, or null if it couldn't be created.</returns>
        public static PrimitiveEllipse ArcFromPointsAndIncludedAngle(Point p1, Point p2, double includedAngle, VertexDirection vertexDirection)
        {
            if (p1.Z != p2.Z)
            {
                throw new InvalidOperationException("only simple planar arcs are currently supported");
            }

            if (includedAngle < 0.0 || includedAngle >= MathHelper.ThreeSixty)
            {
                throw new ArgumentOutOfRangeException(nameof(includedAngle));
            }

            // given the following diagram:
            //
            //                p1
            //               -)
            //            -  |  )
            //        -      |    )
            //    -          |     )
            // O ------------|C----T
            //    -          |  x  )
            //        -      |    )
            //            -  |  )
            //               -)
            //               p2
            //
            // where O is the center of the circle, C is the midpoint between p1 and p2, the
            // distance x is the amount that C would have to be offset perpendicular to the
            // line p1p2 to find the third point T from which we can calculate the arcs

            // first, find the required radius
            // there is an isoceles triangle created between the two points and the center which means splitting that there's
            // a right triangle whose hypotenuse is a radius of the circle and the opposite side is half of the distance between
            // the points.
            var incAngleRad = includedAngle * MathHelper.DegreesToRadians;
            var halfAngle = incAngleRad / 2.0;
            var otherLength = (p2 - p1).Length / 2.0;
            // since sin(theta) = opposite / hypotenuse => opposite / sin(theta) = hypotenuse
            var radius = otherLength / Math.Sin(halfAngle); // hypotenuse length

            // then, given that Op1C is a right triangle and that the line OC has a length of r - x and assuming y is the distance
            // between the point C and p1, we get
            // r^2 = (r-x)^2 + y^2 => x = r - sqrt(r^2 - y^2)
            var midpoint = (p1 + p2) / 2.0;
            var y = otherLength;
            var xOffset = radius - Math.Sqrt((radius * radius) - (y * y));

            // for angles greater than 180 degrees, the offset point is really much farther away
            if (includedAngle >= MathHelper.OneEighty)
                xOffset = radius + radius - xOffset;

            // now offset the midpoint by x (both directions) perpendicular to the line p1p2 and compute the arcs
            var chordVector = p2 - p1;
            var offsetVector = new Vector(-chordVector.Y, chordVector.X, chordVector.Z).Normalize() * xOffset;
            var possibleMidpoint1 = midpoint + offsetVector;
            var possibleMidpoint2 = midpoint - offsetVector;

            // now construct like normal
            var arc = ThreePointArc(p1, possibleMidpoint1, p2, idealNormal: Vector.ZAxis);
            var startPoint = arc.StartPoint();
            if (startPoint.CloseTo(p1) ^ vertexDirection != VertexDirection.CounterClockwise)
            {
                // arc is correct
            }
            else
            {
                arc = ThreePointArc(p1, possibleMidpoint2, p2, idealNormal: Vector.ZAxis);
            }

            return arc;
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
            return new PrimitiveEllipse(center, new Vector(a, 0.0, 0.0), Vector.ZAxis, b / a, 0, 360);
        }
    }
}
