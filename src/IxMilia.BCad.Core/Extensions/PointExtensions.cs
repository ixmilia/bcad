using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using C = IxMilia.Converters;

namespace IxMilia.BCad.Extensions
{
    public static class PointExtensions
    {
        public static C.Vector ToConverterVector(this Point point) => new C.Vector(point.X, point.Y, point.Z);

        public static Point ToPoint(this C.Vector v) => new Point(v.X, v.Y, v.Z);

        public static bool CloseTo(this Point expected, Point actual, double epsilon = MathHelper.Epsilon)
        {
            return MathHelper.CloseTo(expected.X, actual.X, epsilon)
                && MathHelper.CloseTo(expected.Y, actual.Y, epsilon)
                && MathHelper.CloseTo(expected.Z, actual.Z, epsilon);
        }

        public static bool CloseTo(this Vector expected, Vector actual, double epsilon = MathHelper.Epsilon)
        {
            return MathHelper.CloseTo(expected.X, actual.X, epsilon)
                && MathHelper.CloseTo(expected.Y, actual.Y, epsilon)
                && MathHelper.CloseTo(expected.Z, actual.Z, epsilon);
        }

        public static IEnumerable<PrimitiveLine> GetLinesFromPoints(this IEnumerable<Point> points)
        {
            var lines = new List<PrimitiveLine>();
            var first = points.First();
            var last = first;
            foreach (var point in points.Skip(1))
            {
                var line = new PrimitiveLine(last, point);
                lines.Add(line);
                last = point;
            }

            var lastLine = new PrimitiveLine(last, first);
            lines.Add(lastLine);

            return lines;
        }

        public static bool PolygonContains(this IEnumerable<Point> points, Point point)
        {
            return points.GetLinesFromPoints().PolygonContains(point);
        }

        public static bool PolygonContains(this IEnumerable<IPrimitive> primitives, Point point)
        {
            // TODO: this kind of ray casing can fail if the ray and a primitive line are part of the
            // same infinite line or if the ray barely skims the other primitive
            var maxX = primitives.Select(p => Math.Max(p.StartPoint().X, p.EndPoint().X)).Max();
            var dist = Math.Abs(maxX - point.X);
            var ray = new PrimitiveLine(point, new Point(point.X + (dist * 1.1), point.Y, point.Z));
            int intersections = 0;
            foreach (var primitive in primitives)
            {
                intersections += ray.IntersectionPoints(primitive).Count();
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
                    if (verticies.Any(p => rect.Contains(p)))
                    {
                        isContained = true;
                    }
                    else
                    {
                        var selectionLines = new[]
                            {
                                new PrimitiveLine(rect.TopLeft, rect.TopRight),
                                new PrimitiveLine(rect.TopRight, rect.BottomRight),
                                new PrimitiveLine(rect.BottomRight, rect.BottomLeft),
                                new PrimitiveLine(rect.BottomLeft, rect.TopRight)
                            };
                        var reconstructedLines = verticies.Zip(verticies.Skip(1), (a, b) => new PrimitiveLine(a, b)).ToList();
                        var intersectionPoints = reconstructedLines.SelectMany(l => selectionLines.SelectMany(s => s.IntersectionPoints(l))).ToList();
                        if (intersectionPoints.Any())
                        {
                            isContained = true;
                        }
                    }
                }
                else
                {
                    // all points must be in rectangle
                    if (verticies.All(p => rect.Contains(p)))
                    {
                        isContained = true;
                    }
                }
            }

            return isContained;
        }
    }
}
