using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Primitives;
using BCad.Helpers;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
        public static Point ClosestPoint(this PrimitiveLine line, Point point, bool withinBounds = true)
        {
            var v = line.P1;
            var w = line.P2;
            var p = point;
            var wv = w - v;
            var l2 = wv.LengthSquared;
            if (Math.Abs(l2) < MathHelper.Epsilon)
                return v;
            var t = (p - v).Dot(wv) / l2;
            if (withinBounds)
            {
                t = Math.Max(t, 0.0 - MathHelper.Epsilon);
                t = Math.Min(t, 1.0 + MathHelper.Epsilon);
            }

            var result = v + (wv) * t;
            return result;
        }

        public static double Slope(this PrimitiveLine line)
        {
            var denom = line.P2.X - line.P1.X;
            return denom == 0.0 ? double.NaN : (line.P2.Y - line.P1.Y) / denom;
        }

        public static double PerpendicularSlope(this PrimitiveLine line)
        {
            var slope = line.Slope();
            if (double.IsNaN(slope))
                return 0.0;
            else if (slope == 0.0)
                return double.NaN;
            else
                return -1.0 / slope;
        }

        public static PrimitiveLine Transform(this PrimitiveLine line, Matrix3D matrix)
        {
            return new PrimitiveLine(line.P1.Transform(matrix), line.P2.Transform(matrix), line.Color);
        }

        public static IEnumerable<Point> IntersectionPoints(this IPrimitive primitive, IPrimitive other, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    result = ((PrimitiveLine)primitive).IntersectionPoints(other, withinBounds);
                    break;
                case PrimitiveKind.Ellipse:
                    result = ((PrimitiveEllipse)primitive).IntersectionPoints(other, withinBounds);
                    break;
                case PrimitiveKind.Text:
                    result = null;
                    break;
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = null;
                    break;
            }

            return result;
        }

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveLine line, IPrimitive other, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (other.Kind)
            {
                case PrimitiveKind.Line:
                    result = new[] { line.IntersectionPoint((PrimitiveLine)other, withinBounds) };
                    break;
                case PrimitiveKind.Ellipse:
                    result = line.IntersectionPoints((PrimitiveEllipse)other, withinBounds);
                    break;
                case PrimitiveKind.Text:
                    result = null;
                    break;
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = null;
                    break;
            }

            return result;
        }

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveEllipse ellipse, IPrimitive other, bool withinBounds = true)
        {
            return other.IntersectionPoints(ellipse);
        }

        public static Point IntersectionPoint(this PrimitiveLine first, PrimitiveLine second, bool withinSegment = true)
        {
            var minLength = 0.0000000001;

            //http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
            // find real 3D intersection within a minimum distance
            var p1 = first.P1;
            var p2 = first.P2;
            var p3 = second.P1;
            var p4 = second.P2;
            var p13 = p1 - p3;
            var p43 = p4 - p3;

            if (p43.LengthSquared < MathHelper.Epsilon)
                return null;

            var p21 = p2 - p1;
            if (p21.LengthSquared < MathHelper.Epsilon)
                return null;

            var d1343 = p13.Dot(p43);
            var d4321 = p43.Dot(p21);
            var d1321 = p13.Dot(p21);
            var d4343 = p43.Dot(p43);
            var d2121 = p21.Dot(p21);

            var denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < MathHelper.Epsilon)
                return null;

            var num = d1343 * d4321 - d1321 * d4343;
            var mua = num / denom;
            var mub = (d1343 + d4321 * mua) / d4343;

            if (withinSegment)
            {
                if (!MathHelper.Between(0.0, 1.0, mua) ||
                    !MathHelper.Between(0.0, 1.0, mub))
                {
                    return null;
                }
            }

            var connector = new PrimitiveLine((p21 * mua) + p1, (p43 * mub) + p3);
            var cv = connector.P1 - connector.P2;
            if (cv.LengthSquared > minLength)
                return null;

            var point = (Point)((connector.P1 + connector.P2) / 2);
            return point;
        }

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveLine line, PrimitiveEllipse ellipse, bool withinSegment = true)
        {
            var empty = Enumerable.Empty<Point>();

            var l0 = line.P1;
            var l = line.P2 - line.P1;
            var p0 = ellipse.Center;
            var n = ellipse.Normal;

            var right = ellipse.MajorAxis.Normalize();
            var up = ellipse.Normal.Cross(right).Normalize();
            var radiusX = ellipse.MajorAxis.Length;
            var radiusY = radiusX * ellipse.MinorAxisRatio;
            var transform = GenerateUnitCircleProjection(ellipse.Normal, right, up, ellipse.Center, radiusX, radiusY, 1.0);
            var invert = transform;
            invert.Invert();

            var denom = l.Dot(n);
            var num = (p0 - l0).Dot(n);

            var flatPoints = new List<Point>();

            if (Math.Abs(denom) < MathHelper.Epsilon)
            {
                // plane either contains the line entirely or is parallel
                if (Math.Abs(num) < MathHelper.Epsilon)
                {
                    // parallel.  normalize the plane and find the intersection
                    var flatLine = line.Transform(invert);
                    // the ellipse is now centered at the origin with a radius of 1.
                    // find the intersection points then reproject
                    var dv = flatLine.P2 - flatLine.P1;
                    var dx = dv.X;
                    var dy = dv.Y;
                    var dr2 = dx * dx + dy * dy;
                    var D = flatLine.P1.X * flatLine.P2.Y - flatLine.P2.X * flatLine.P1.Y;
                    var det = dr2 - D * D;
                    var points = new List<Point>();
                    if (det < 0.0 || Math.Abs(dr2) < MathHelper.Epsilon)
                    {
                        // no intersection or line is too short
                    }
                    else if (Math.Abs(det) < MathHelper.Epsilon)
                    {
                        // 1 point
                        var x = (D * dy) / dr2;
                        var y = (-D * dx) / dr2;
                        points.Add(new Point(x, y, 0.0));
                    }
                    else
                    {
                        // 2 points
                        var sgn = dy < 0.0 ? -1.0 : 1.0;
                        var det2 = Math.Sqrt(det);
                        points.Add(
                            new Point(
                                (D * dy + sgn * dx * det2) / dr2,
                                (-D * dx + Math.Abs(dy) * det2) / dr2,
                                0.0));
                        points.Add(
                            new Point(
                                (D * dy - sgn * dx * det2) / dr2,
                                (-D * dx - Math.Abs(dy) * det2) / dr2,
                                0.0));
                    }

                    // ensure the points are within appropriate bounds
                    if (withinSegment)
                    {
                        // line test
                        points = points.Where(p =>
                            MathHelper.Between(flatLine.P1.X, flatLine.P2.X, p.X) &&
                            MathHelper.Between(flatLine.P1.Y, flatLine.P2.Y, p.Y)).ToList();

                        // circle test
                        points = (from p in points
                                  let angle = (Math.Atan2(p.Y, p.X) * MathHelper.RadiansToDegrees).CorrectAngleDegrees()
                                  where MathHelper.Between(ellipse.StartAngle, ellipse.EndAngle, angle)
                                     || MathHelper.Between(ellipse.StartAngle - 360.0, ellipse.EndAngle, angle)
                                     || MathHelper.Between(ellipse.StartAngle, ellipse.EndAngle + 360.0, angle)
                                  select p)
                                  .ToList();
                    }

                    return points.Select(p => p.Transform(transform));
                }
            }
            else
            {
                // otherwise line and plane intersect in only 1 point, p
                var d = num / denom;
                var p = l * d + l0;

                // verify within the line segment
                if (withinSegment && !MathHelper.Between(0.0, 1.0, d))
                {
                    return empty;
                }

                // p is the point of intersection.  verify if on transformed unit circle
                var unitVector = (Vector)p.Transform(invert);
                if (Math.Abs(unitVector.Length - 1.0) < MathHelper.Epsilon)
                {
                    // point is on the unit circle
                    if (withinSegment)
                    {
                        // verify within the angles specified
                        var angle = Math.Atan2(unitVector.Y, unitVector.X) * MathHelper.RadiansToDegrees;
                        if (MathHelper.Between(ellipse.StartAngle, ellipse.EndAngle, angle.CorrectAngleDegrees()))
                        {
                            return new[] { p };
                        }
                    }
                    else
                    {
                        return new[] { p };
                    }
                }
            }

            // point is not on unit circle
            return empty;
        }

        public static Matrix3D GenerateUnitCircleProjection(Vector normal, Vector right, Vector up, Point center, double scaleX, double scaleY, double scaleZ)
        {
            var transformation = Matrix3D.Identity;
            transformation.M11 = right.X;
            transformation.M12 = right.Y;
            transformation.M13 = right.Z;
            transformation.M21 = up.X;
            transformation.M22 = up.Y;
            transformation.M23 = up.Z;
            transformation.M31 = normal.X;
            transformation.M32 = normal.Y;
            transformation.M33 = normal.Z;
            transformation.OffsetX = center.X;
            transformation.OffsetY = center.Y;
            transformation.OffsetZ = center.Z;
            var scale = Matrix3D.Identity;
            scale.M11 = scaleX;
            scale.M22 = scaleY;
            scale.M33 = scaleZ;
            return scale * transformation;
        }
    }
}
