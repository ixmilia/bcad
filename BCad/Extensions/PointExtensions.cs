using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Point ToPoint(this System.Windows.Point point)
        {
            return new Point(point.X, point.Y, 0);
        }

        public static System.Windows.Point ToWindowsPoint(this Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
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

        public static bool Contains(this Rect rect, IEnumerable<Point> verticies, bool includePartial)
        {
            if (verticies == null)
                throw new ArgumentNullException("verticies");
            if (!verticies.Any())
                throw new InvalidOperationException("You must specify verticies");

            // first check for whole-sale bounding rectangle containment
            var first = verticies.First();
            var left = first.X;
            var right = first.X;
            var top = first.Y;
            var bottom = first.Y;
            foreach (var v in verticies.Skip(1))
            {
                if (v.X < left)
                    left = v.X;
                if (v.X > right)
                    right = v.X;
                if (v.Y < top)
                    top = v.Y;
                if (v.Y > bottom)
                    bottom = v.Y;
            }

            var screenRect = new Rect(left, top, right - left, bottom - top);
            bool isContained = false;

            if (rect.Contains(screenRect))
            {
                // regardless of selection type, this will match
                isContained = true;
            }
            else
            {
                // project all line segments to screen space
                if (includePartial)
                {
                    // if any point is in the rectangle OR any segment intersects a rectangle edge
                    if (verticies.Any(p => rect.Contains(p.ToWindowsPoint())))
                    {
                        isContained = true;
                    }
                    else
                    {
                        var selectionLines = new[]
                            {
                                new PrimitiveLine(rect.TopLeft.ToPoint(), rect.TopRight.ToPoint()),
                                new PrimitiveLine(rect.TopRight.ToPoint(), rect.BottomRight.ToPoint()),
                                new PrimitiveLine(rect.BottomRight.ToPoint(), rect.BottomLeft.ToPoint()),
                                new PrimitiveLine(rect.BottomLeft.ToPoint(), rect.TopRight.ToPoint())
                            };
                        if (verticies
                            .Zip(verticies.Skip(1), (a, b) => new PrimitiveLine(a, b))
                            .Any(l => selectionLines.Any(s => s.IntersectionPoint(l) != null)))
                        {
                            isContained = true;
                        }
                    }
                }
                else
                {
                    // all points must be in rectangle
                    if (verticies.All(p => rect.Contains(p.ToWindowsPoint())))
                    {
                        isContained = true;
                    }
                }
            }

            return isContained;
        }
    }
}
