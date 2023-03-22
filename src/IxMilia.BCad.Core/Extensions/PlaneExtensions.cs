using System;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Extensions
{
    public static class PlaneExtensions
    {
        public static bool Contains(this Plane plane, Point p)
        {
            var v = p - plane.Point;
            return v.IsZeroVector || v.IsOrthoganalTo(plane.Normal);
        }

        public static bool Contains(this Plane plane, IPrimitive primitive)
        {
            return primitive.MapPrimitive<bool>(
                ellipse =>
                    plane.Contains(ellipse.Center) &&
                    plane.Normal.IsParallelTo(ellipse.Normal) &&
                    plane.Normal.IsOrthoganalTo(ellipse.MajorAxis),
                line =>
                    plane.Contains(line.P1) &&
                    plane.Contains(line.P2),
                point => plane.Contains(point.Location),
                text =>
                    plane.Contains(text.Location) &&
                    plane.Normal.IsParallelTo(text.Normal),
                bezier =>
                    plane.Contains(bezier.P1) &&
                    plane.Contains(bezier.P2) &&
                    plane.Contains(bezier.P3) &&
                    plane.Contains(bezier.P4),
                image => plane.Contains(image.Location)
            );
        }

        public static bool Contains(this Plane plane, Entity entity, DrawingSettings settings)
        {
            return entity.GetPrimitives(settings).All(p => plane.Contains(p));
        }

        public static Matrix4 ToXYPlaneProjection(this Plane plane)
        {
            var right = Vector.XAxis;
            if (plane.Normal.IsParallelTo(right))
                right = Vector.YAxis;
            var up = plane.Normal.Cross(right).Normalize();
            right = up.Cross(plane.Normal).Normalize();
            var matrix = Matrix4.FromUnitCircleProjection(plane.Normal, right, up, Point.Origin, 1.0, 1.0, 1.0);
            return matrix;
        }

        public static Point ToXYPlane(this Plane plane, Point point)
        {
            return plane.ToXYPlaneProjection().Transform(point);
        }

        public static Point FromXYPlane(this Plane plane, Point point)
        {
            var matrix = plane.ToXYPlaneProjection().Inverse();
            return matrix.Transform(point);
        }
    }
}
