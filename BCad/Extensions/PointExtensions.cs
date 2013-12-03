using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Helpers;
using BCad.Primitives;
using SharpDX;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Point ToPoint(this System.Windows.Point point)
        {
            return new Point(point.X, point.Y, 0);
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

        public static IEnumerable<Point> ConvexHull(this IEnumerable<Point> verticies)
        {
            var verts = verticies.Distinct();
            var hull = new List<Point>();
            var remaining = new List<Point>(verts);

            // min x is the first value
            var start = verts.OrderBy(v => v.X).First();
            hull.Add(start);

            var ninety = MathHelper.PI / 2.0;
            Func<Point, Point, double> polarAngle = (pivot, arm) =>
                {
                    var delta = arm - pivot;
                    if (delta.LengthSquared == 0.0)
                        return double.MaxValue;
                    else
                    {
                        var angle = (Math.Atan2(delta.Y, delta.X) - ninety) * -1.0;
                        return angle < 0.0 ? angle + MathHelper.TwoPI : angle;
                    }
                };

            Point endPoint;
            do
            {
                // find greatest polar angle in remaining points
                endPoint = remaining.OrderBy(r => polarAngle(hull.Last(), r)).First();
                hull.Add(endPoint);
                remaining.Remove(endPoint);
            } while (endPoint != start);

            return hull;
        }
    }
}
