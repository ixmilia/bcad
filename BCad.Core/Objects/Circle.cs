using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public class Circle : IObject, IPrimitive
    {
        public Point Center { get; private set; }

        public Vector Normal { get; private set; }

        public double Radius { get; private set; }

        public Color Color { get; private set; }

        public Circle(Point center, double radius, Vector normal, Color color)
        {
            Center = center;
            Radius = radius;
            Normal = normal;
            Color = color;
        }

        public IEnumerable<IPrimitive> GetPrimitives()
        {
            yield return this;
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new CenterPoint(Center);
            
            // compute quadrants with normal
            var norm = Normal;
            norm.Normalize();

            if (norm == Vector.ZAxis)
            {
                yield return new QuadrantPoint((Center + new Vector(Radius, 0.0, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(-Radius, 0.0, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, Radius, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, -Radius, 0.0)).ToPoint());
            }
            else if (norm == Vector.YAxis)
            {
                yield return new QuadrantPoint((Center + new Vector(Radius, 0.0, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(-Radius, 0.0, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, 0.0, Radius)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, 0.0, -Radius)).ToPoint());
            }
            else if (norm == Vector.XAxis)
            {
                yield return new QuadrantPoint((Center + new Vector(0.0, Radius, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, -Radius, 0.0)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, 0.0, Radius)).ToPoint());
                yield return new QuadrantPoint((Center + new Vector(0.0, 0.0, -Radius)).ToPoint());
            }
            else
            {
                // arbitrary normal.  find a point near center, not in the direction of the normal
                var p = (Center + Vector.XAxis).ToPoint();
                var other = p - Center;
                other.Normalize();

                var axis1 = other.Cross(norm);
                axis1.Normalize();
                var axis2 = axis1.Cross(norm);
                axis2.Normalize();

                yield return new QuadrantPoint((Center + (axis1 * Radius)).ToPoint());
                yield return new QuadrantPoint((Center + (axis1 * -Radius)).ToPoint());
                yield return new QuadrantPoint((Center + (axis2 * Radius)).ToPoint());
                yield return new QuadrantPoint((Center + (axis2 * -Radius)).ToPoint());
            }
        }

        public Circle Update(Point center = null, double? radius = null, Vector normal = null, Color color = null)
        {
            return new Circle(
                center ?? this.Center,
                radius ?? this.Radius,
                normal ?? this.Normal,
                color ?? this.Color);
        }
    }
}
