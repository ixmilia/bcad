using System;
using System.Linq;
using BCad.Entities;
using BCad.Helpers;

namespace BCad.Extensions
{
    public static class EntityExtensions
    {
        public static bool EquivalentTo(this Arc arc, Entity entity)
        {
            var other = entity as Arc;
            if (other != null)
            {

                return arc.Center.CloseTo(other.Center)
                    && arc.Color == other.Color
                    && MathHelper.CloseTo(arc.EndAngle, other.EndAngle)
                    && arc.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(arc.Radius, other.Radius)
                    && MathHelper.CloseTo(arc.StartAngle, other.StartAngle);
            }

            return false;
        }

        public static bool EquivalentTo(this Circle circle, Entity entity)
        {
            var other = entity as Circle;
            if (other != null)
            {
                return circle.Center.CloseTo(other.Center)
                    && circle.Color == other.Color
                    && circle.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(circle.Radius, other.Radius);
            }

            return false;
        }

        public static bool EquivalentTo(this Ellipse el, Entity entity)
        {
            var other = entity as Ellipse;
            if (other != null)
            {
                return el.Center.CloseTo(other.Center)
                    && el.Color == other.Color
                    && MathHelper.CloseTo(el.EndAngle, other.EndAngle)
                    && el.MajorAxis.CloseTo(other.MajorAxis)
                    && MathHelper.CloseTo(el.MinorAxisRatio, other.MinorAxisRatio)
                    && el.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(el.StartAngle, other.StartAngle);
            }

            return false;
        }

        public static bool EquivalentTo(this Line line, Entity entity)
        {
            var other = entity as Line;
            if (other != null)
            {
                return line.Color == other.Color
                    && line.P1.CloseTo(other.P1)
                    && line.P2.CloseTo(other.P2);
            }

            return false;
        }

        public static bool EquivalentTo(this Entity a, Entity b)
        {
            if (a is Arc)
                return ((Arc)a).EquivalentTo(b);
            else if (a is Circle)
                return ((Circle)a).EquivalentTo(b);
            else if (a is Ellipse)
                return ((Ellipse)a).EquivalentTo(b);
            else if (a is Line)
                return ((Line)a).EquivalentTo(b);
            else
                throw new NotSupportedException("Unsupported entity type");
        }

        public static Matrix4 GetUnitCircleProjection(this Entity entity)
        {
            Matrix4 matrix;
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    matrix = arc.FromUnitCircle;
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    matrix = circle.FromUnitCircle;
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)entity;
                    matrix = el.FromUnitCircle;
                    break;
                default:
                    throw new ArgumentException("entity");
            }

            return matrix;
        }

        public static bool ContainsPoint(this Polyline polyline, Point point)
        {
            // TODO: update to handle arcs
            return polyline.Vertices.Select(v => v.Location).PolygonContains(point);
        }

        public static Point MidPoint(this Line line)
        {
            return line.GetPrimitives().Single().MidPoint();
        }
    }
}
