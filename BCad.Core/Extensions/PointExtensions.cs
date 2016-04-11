using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static bool CloseTo(this Point expected, Point actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }

        public static bool CloseTo(this Vector expected, Vector actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }

        public static bool PolygonContains(this IEnumerable<Point> verticies, Point point)
        {
            var arr = verticies.ToArray();
            var maxX = verticies.Select(v => v.X).Max();
            var dist = Math.Abs(maxX - point.X);
            var ray = new PrimitiveLine(point, new Point(point.X + (dist * 1.1), point.Y, point.Z));
            int intersections = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                var segment = new PrimitiveLine(arr[i], arr[i + 1]);
                if (ray.IntersectionPoint(segment) != null)
                    intersections++;
            }

            return intersections % 2 == 1;
        }
    }
}
