using System;
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
            Vector normal, right, up;
            Point center;
            double scaleX, scaleY;
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    normal = arc.Normal;
                    right = Vector.RightVectorFromNormal(normal);
                    up = normal.Cross(right).Normalize();
                    center = arc.Center;
                    scaleX = scaleY = arc.Radius;
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    normal = circle.Normal;
                    right = Vector.RightVectorFromNormal(normal);
                    up = normal.Cross(right).Normalize();
                    center = circle.Center;
                    scaleX = scaleY = circle.Radius;
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)entity;
                    normal = el.Normal;
                    right = el.MajorAxis.Normalize();
                    up = normal.Cross(right).Normalize();
                    center = el.Center;
                    scaleX = el.MajorAxis.Length;
                    scaleY = scaleX * el.MinorAxisRatio;
                    break;
                default:
                    throw new ArgumentException("entity");
            }

            return PrimitiveExtensions.FromUnitCircleProjection(normal, right, up, center, scaleX, scaleY, 1.0);
        }

        public static Point MidPoint(this Line line)
        {
            return (line.P1 + line.P2) / 2;
        }
    }
}
