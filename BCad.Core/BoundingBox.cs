using System;
using System.Diagnostics;

namespace BCad
{
    public struct BoundingBox
    {
        public Point MinimumPoint { get; set; }

        public Vector Size { get; set; }

        public BoundingBox(Point minimumPoint, Vector size)
            : this()
        {
            this.MinimumPoint = minimumPoint;
            this.Size = size;
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
    }
}
