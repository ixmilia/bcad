using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IxMilia.BCad
{
    public struct BoundingBox
    {
        public Point MinimumPoint { get; set; }

        public Point MaximumPoint { get { return MinimumPoint + Size; } }

        public Vector Size { get; set; }

        public BoundingBox(Point minimumPoint, Vector size)
            : this()
        {
            this.MinimumPoint = minimumPoint;
            this.Size = size;
        }

        public bool Intersects(BoundingBox other)
        {
            var left1 = Math.Max(MinimumPoint.X, other.MinimumPoint.X);
            var left2 = Math.Min(MinimumPoint.X + Size.X, other.MinimumPoint.X + other.Size.X);
            var top1 = Math.Max(MinimumPoint.Y, other.MinimumPoint.Y);
            var top2 = Math.Min(MinimumPoint.Y + Size.Y, other.MinimumPoint.Y + other.Size.Y);
            var back1 = Math.Max(MinimumPoint.Z, other.MinimumPoint.Z);
            var back2 = Math.Min(MinimumPoint.Z + Size.Z, other.MinimumPoint.Z + other.Size.Z);
            return left2 >= left1 && top2 >= top1 && back2 >= back1;
        }

        public static BoundingBox FromPoints(params Point[] points)
        {
            Debug.Assert(points.Length > 0);
            double minX, minY, minZ, maxX, maxY, maxZ;
            minX = maxX = points[0].X;
            minY = maxY = points[0].Y;
            minZ = maxZ = points[0].Z;

            for (int i = 1; i < points.Length; i++)
            {
                minX = Math.Min(minX, points[i].X);
                maxX = Math.Max(maxX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxY = Math.Max(maxY, points[i].Y);
                minZ = Math.Min(minZ, points[i].Z);
                maxZ = Math.Max(maxZ, points[i].Z);
            }

            var min = new Point(minX, minY, minZ);
            var max = new Point(maxX, maxY, maxZ);
            return new BoundingBox(min, max - min);
        }

        public static BoundingBox Includes(IEnumerable<BoundingBox> boxes)
        {
            if (!boxes.Any())
                return new BoundingBox();

            var min = boxes.First().MinimumPoint;
            var max = boxes.First().MaximumPoint;
            foreach (var box in boxes.Skip(1))
            {
                if (box.MinimumPoint.X < min.X || box.MinimumPoint.Y < min.Y || box.MinimumPoint.Z < min.Z)
                    min = box.MinimumPoint;
                if (box.MaximumPoint.X > max.X || box.MaximumPoint.Y > max.Y || box.MaximumPoint.Z > max.Z)
                    max = box.MaximumPoint;
            }

            return new BoundingBox(min, max - min);
        }
    }
}
