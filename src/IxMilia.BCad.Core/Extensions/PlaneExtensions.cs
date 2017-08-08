// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    return plane.Contains(el.Center)
                        && plane.Normal.IsParallelTo(el.Normal)
                        && plane.Normal.IsOrthoganalTo(el.MajorAxis);
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    return plane.Contains(line.P1) && plane.Contains(line.P2);
                case PrimitiveKind.Point:
                    var p = (PrimitivePoint)primitive;
                    return plane.Contains(p.Location);
                case PrimitiveKind.Text:
                    var t = (PrimitiveText)primitive;
                    return plane.Contains(t.Location)
                        && plane.Normal.IsParallelTo(t.Normal);
                default:
                    throw new ArgumentException("primitive.Kind");
            }
        }

        public static bool Contains(this Plane plane, Entity entity)
        {
            return entity.GetPrimitives().All(p => plane.Contains(p));
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
