using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using SlimDX;
using SlimDX.Direct3D9;

namespace BCad.UI
{
    public static class Direct3DExtensions
    {
        public static Vector3 ToVector3(this System.Windows.Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, 0.0f);
        }

        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Vector3 ToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
        }

        public static Vector3 ToVector3(this Point3D point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Vector ToVector(this Vector3 vector)
        {
            return new Vector(vector.X, vector.Y, vector.Z);
        }

        public static Point ToPoint(this Vector3 point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        public static System.Windows.Point ToWindowsPoint(this Vector3 point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static Matrix ToMatrix(this Matrix3D matrix)
        {
            return new Matrix()
            {
                M11 = (float)matrix.M11,
                M12 = (float)matrix.M12,
                M13 = (float)matrix.M13,
                M14 = (float)matrix.M14,
                M21 = (float)matrix.M21,
                M22 = (float)matrix.M22,
                M23 = (float)matrix.M23,
                M24 = (float)matrix.M24,
                M31 = (float)matrix.M31,
                M32 = (float)matrix.M32,
                M33 = (float)matrix.M33,
                M34 = (float)matrix.M34,
                M41 = (float)matrix.OffsetX,
                M42 = (float)matrix.OffsetY,
                M43 = (float)matrix.OffsetZ,
                M44 = (float)matrix.M44,
            };
        }

        public static SlimDX.BoundingBox GetBoundingBox(this Mesh mesh)
        {
            var count = mesh.VertexCount;
            var bpv = mesh.BytesPerVertex;
            var verts = new Vector3[count];
            var ds = mesh.LockVertexBuffer(LockFlags.ReadOnly);
            for (int i = 0; ds.Position < ds.Length && i < count; i++)
            {
                var old = ds.Position;
                verts[i] = ds.Read<Vector3>();
                ds.Position = old + bpv;
            }

            mesh.UnlockVertexBuffer();
            return SlimDX.BoundingBox.FromPoints(verts);
        }

        public static bool Contains(this Rect rect, Vector3[] lineSegments, Func<Vector3, Vector3> project, bool includePartial)
        {
            var boundingBox = SlimDX.BoundingBox.FromPoints(lineSegments);

            // project all 8 bounding box coordinates to the screen and create a bigger bounding rectangle
            var projectedBox = boundingBox.GetCorners();
            for (int i = 0; i < projectedBox.Length; i++)
            {
                projectedBox[i] = project(projectedBox[i]);
            }
            var screenRect = projectedBox.GetBoundingRectangle();
            bool isContained = false;

            if (rect.Contains(screenRect))
            {
                // regardless of selection type, this will match
                isContained = true;
            }
            else
            {
                // project all line segments to screen space
                var projectedPoints = lineSegments.Select(v => project(v).ToWindowsPoint());

                if (includePartial)
                {
                    // if any point is in the rectangle OR any segment intersects a rectangle edge
                    if (projectedPoints.Any(p => rect.Contains(p)))
                    {
                        isContained = true;
                    }
                    else
                    {
                        var selectionLines = new[]
                            {
                                new PrimitiveLine(new Point(rect.TopLeft), new Point(rect.TopRight)),
                                new PrimitiveLine(new Point(rect.TopRight), new Point(rect.BottomRight)),
                                new PrimitiveLine(new Point(rect.BottomRight), new Point(rect.BottomLeft)),
                                new PrimitiveLine(new Point(rect.BottomLeft), new Point(rect.TopRight))
                            };
                        if (projectedPoints
                            .Zip(projectedPoints.Skip(1), (a, b) => new PrimitiveLine(new Point(a), new Point(b)))
                            .Any(l => selectionLines.Any(s => s.IntersectionPoint(l) != null)))
                        {
                            isContained = true;
                        }
                    }
                }
                else
                {
                    // all points must be in rectangle
                    if (projectedPoints.All(p => rect.Contains(p)))
                    {
                        isContained = true;
                    }
                }
            }

            return isContained;
        }

        public static Rect GetBoundingRectangle(this Vector3[] points)
        {
            Debug.Assert(points.Length > 0);
            float minX, minY, maxX, maxY;
            minX = maxX = points[0].X;
            minY = maxY = points[0].Y;
            for (int i = 1; i < points.Length; i++)
            {
                minX = Math.Min(minX, points[i].X);
                maxX = Math.Max(maxX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxY = Math.Max(maxY, points[i].Y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public static bool Contains(this IEnumerable<Vector3> verticies, Vector3 point)
        {
            var arr = verticies.ToArray();
            var maxX = verticies.Select(v => v.X).Max();
            var dist = maxX - point.X;
            var ray = new PrimitiveLine(point.ToPoint(), new Point(point.X + (dist * 1.1), point.Y, point.Z));
            int intersections = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                var segment = new PrimitiveLine(arr[i].ToPoint(), arr[i + 1].ToPoint());
                if (ray.IntersectionPoint(segment) != null)
                    intersections++;
            }

            return intersections % 2 == 1;
        }

        public static IEnumerable<Vector3> ConvexHull(this IEnumerable<Vector3> verticies)
        {
            var verts = verticies.Distinct();
            var hull = new List<Vector3>();
            var remaining = new List<Vector3>(verts);

            // min x is the first value
            var start = verts.OrderBy(v => v.X).First();
            hull.Add(start);

            var ninety = MathHelper.PI / 2.0;
            Func<Vector3, Vector3, double> polarAngle = (pivot, arm) =>
                {
                    var delta = arm - pivot;
                    if (delta.LengthSquared() == 0.0f)
                        return double.MaxValue;
                    else
                    {
                        var angle = (Math.Atan2(delta.Y, delta.X) - ninety) * -1.0;
                        return angle < 0.0 ? angle + MathHelper.TwoPI : angle;
                    }
                };

            Vector3 endPoint;
            do
            {
                // find greatest polar angle in remaining points
                endPoint = remaining.OrderBy(r => polarAngle(hull.Last(), r)).First();
                hull.Add(endPoint);
                remaining.Remove(endPoint);
            } while (endPoint != start);

            return hull;
        }

        public static Tuple<double, Point> ClosestPoint(this Point point, Vector3[] verticies, Func<Vector3, Vector3> project)
        {
            var points = from i in Enumerable.Range(0, verticies.Length - 1)
                         // translate line segment to screen coordinates
                         let p1 = project(verticies[i]).ToPoint()
                         let p2 = project(verticies[i + 1]).ToPoint()
                         let segment = new PrimitiveLine(p1, p2)
                         let closest = segment.ClosestPoint(point)
                         let dist = (closest - point).LengthSquared
                         orderby dist
                         // simple unproject via interpolation
                         let pct = (closest - p1).Length / (p2 - p1).Length
                         let vec = (Vector)(verticies[i + 1] - verticies[i]).ToPoint()
                         let newLen = vec.Length * pct
                         let offset = vec.Normalize() * newLen
                         select Tuple.Create(dist, verticies[i].ToPoint() + offset);
            var selected = points.FirstOrDefault();
            return selected;
        }
    }
}
