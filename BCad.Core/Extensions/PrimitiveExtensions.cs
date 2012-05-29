using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Primitives;
using BCad.Helpers;
using System.Diagnostics;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
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

        public static IEnumerable<Point> IntersectionPoints(this IPrimitive primitive, IPrimitive other, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    result = ((PrimitiveLine)primitive).IntersectionPoints(other, withinBounds);
                    break;
                case PrimitiveKind.Ellipse:
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
                    result = new[] { line.IntersectionXY((PrimitiveLine)other, withinBounds) };
                    break;
                case PrimitiveKind.Ellipse:
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = null;
                    break;
            }

            return result;
        }

        public static Point IntersectionXY(this PrimitiveLine first, PrimitiveLine second, bool withinSegment = true)
        {
            //var epsilon = 0.0000000001;
            //var minLength = 0.0000000001;

            //var p0 = first.P1;
            //var u = first.P2 - first.P1;
            //var q0 = second.P1;
            //var v = second.P2 - second.P1;
            //var w0 = q0 - p0;

            //var a = u.Dot(u);
            //var b = u.Dot(v);
            //var c = v.Dot(v);
            //var d = u.Dot(w0);
            //var e = v.Dot(w0);

            //var denom = a * c - b * b;

            //if (denom == 0.0)
            //    return null; // parallel

            //var sc = (b * e - c * d) / denom;
            //var tc = (a * e - b * d) / denom;

            //var ps = p0 + (u * sc);
            //var qt = q0 + (v * tc);
            //var l = (ps - qt).LengthSquared;

            //if (l > minLength)
            //    return null; // too far apart

            //return (ps + qt) / 2.0;

            //http://paulbourke.net/geometry/lineline3d/calclineline.cs
            // find real 3D intersection within a minimum distance
            //var p1 = first.P1;
            //var p2 = first.P2;
            //var p3 = second.P1;
            //var p4 = second.P2;
            //var p13 = p1 - p3;
            //var p43 = p4 - p3;

            //if (p43.LengthSquared < epsilon)
            //    return null;

            //var p21 = p2 - p1;
            //if (p21.LengthSquared < epsilon)
            //    return null;

            //var d1343 = p13.Dot(p43);
            //var d4321 = p43.Dot(p21);
            //var d1321 = p13.Dot(p21);
            //var d4343 = p43.Dot(p43);
            //var d2121 = p21.Dot(p21);

            //var denom = d2121 * d4343 - d4321 * d4321;
            //if (Math.Abs(denom) < epsilon)
            //    return null;

            //var num = d1343 * d4321 - d1321 * d4343;
            //var mua = num / denom;
            //var mub = (d1343 + d4321 * mua) / d4343;

            //var connector = new PrimitiveLine((p21 * mua) + p1, (p43 * mub) + p3);
            //var cv = connector.P1 - connector.P2;
            //if (cv.LengthSquared > minLength)
            //    return null;

            //var point = (Point)((connector.P1 + connector.P2) / 2);
            //if (withinSegment)
            //{
            //    if (!MathHelper.Between(Math.Min(p1.X, p2.X), Math.Max(p1.X, p2.X), point.X) ||
            //        !MathHelper.Between(Math.Min(p1.Y, p2.Y), Math.Max(p1.Y, p2.Y), point.Y) ||
            //        !MathHelper.Between(Math.Min(p1.Z, p2.Z), Math.Max(p1.Z, p2.Z), point.Z) ||
            //        !MathHelper.Between(Math.Min(p3.X, p4.X), Math.Max(p3.X, p4.X), point.X) ||
            //        !MathHelper.Between(Math.Min(p3.Y, p4.Y), Math.Max(p3.Y, p4.Y), point.Y) ||
            //        !MathHelper.Between(Math.Min(p3.Z, p4.Z), Math.Max(p3.Z, p4.Z), point.Z))
            //    {
            //        point = null;
            //    }
            //}

            //return point;


            if (first.P1.Z != first.P2.Z || second.P1.Z != second.P2.Z || first.P1.Z != second.P1.Z)
            {
                // not all in the same plane
                return null;
            }

            var m1 = first.Slope();
            var m2 = second.Slope();
            var b1 = first.P1.Y - m1 * first.P1.X;
            var b2 = second.P1.Y - m2 * second.P1.X;
            var z = first.P1.Z;
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
                    result = new Point(first.P1.X, m2 * first.P1.X + b2, z);
                }
            }
            else
            {
                // first line is not vertial
                if (double.IsNaN(m2))
                {
                    // second line is vertical
                    result = new Point(second.P1.X, m1 * second.P1.X + b1, z);
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
                if (!MathHelper.Between(Math.Min(first.P1.X, first.P2.X), Math.Max(first.P1.X, first.P2.X), result.X) ||
                    !MathHelper.Between(Math.Min(first.P1.Y, first.P2.Y), Math.Max(first.P1.Y, first.P2.Y), result.Y) ||
                    !MathHelper.Between(Math.Min(second.P1.X, second.P2.X), Math.Max(second.P1.X, second.P2.X), result.X) ||
                    !MathHelper.Between(Math.Min(second.P1.Y, second.P2.Y), Math.Max(second.P1.Y, second.P2.Y), result.Y))
                {
                    result = null;
                }
            }

            return result;
        }
    }
}
