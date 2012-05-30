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
                    result = new[] { line.IntersectionPoint((PrimitiveLine)other, withinBounds) };
                    break;
                case PrimitiveKind.Ellipse:
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = null;
                    break;
            }

            return result;
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

            Func<Point, Point, Point, Point, double> d = (m, n, o, p) =>
            {
                return (m.X - n.X) * (o.X - p.X) + (m.Y - n.Y) * (o.Y - p.Y) + (m.Z - n.Z) * (o.Z - p.Z);
            };

            var d1343 = d(p1, p3, p4, p3);
            var d4321 = d(p4, p3, p2, p1);
            var d1321 = d(p1, p3, p2, p1);
            var d4343 = d(p4, p3, p4, p3);
            var d2121 = d(p2, p1, p2, p1);

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
    }
}
