using System;
using System.Collections.Generic;
using BCad.Helpers;
using BCad.Primitives;
using BCad.SnapPoints;
using BCad.Extensions;

namespace BCad.Entities
{
    public class Circle : Entity
    {
        private readonly Point center;
        private readonly Vector normal;
        private readonly double radius;
        private readonly Point quadrant1;
        private readonly Point quadrant2;
        private readonly Point quadrant3;
        private readonly Point quadrant4;
        private readonly PrimitiveEllipse primitive;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Center { get { return center; } }

        public Vector Normal { get { return normal; } }

        public double Radius { get { return radius; } }

        public Matrix4 FromUnitCircle { get { return primitive.FromUnitCircle; } }

        public Circle(Point center, double radius, Vector normal, IndexedColor color, object tag = null)
            : base(color, tag)
        {
            this.center = center;
            this.radius = radius;
            this.normal = normal;

            var right = Vector.RightVectorFromNormal(this.normal);
            var points = TransformedPoints(this.center, this.normal, right, this.radius, this.radius, 0, 90, 180, 270);
            quadrant1 = points[0];
            quadrant2 = points[1];
            quadrant3 = points[2];
            quadrant4 = points[3];

            this.primitive = new PrimitiveEllipse(Center, Radius, Normal, Color);
            this.primitives = new IPrimitive[] { this.primitive };
            this.snapPoints = new SnapPoint[]
            {
                new CenterPoint(Center),
                new QuadrantPoint(quadrant1),
                new QuadrantPoint(quadrant2),
                new QuadrantPoint(quadrant3),
                new QuadrantPoint(quadrant4)
            };
            this.boundingBox = BoundingBox.FromPoints(points);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case CenterText:
                    return Center;
                case NormalText:
                    return Normal;
                case RadiusText:
                    return Radius;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Circle; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Circle Update(
            Optional<Point> center = default(Optional<Point>),
            Optional<double> radius = default(Optional<double>),
            Optional<Vector> normal = default(Optional<Vector>),
            Optional<IndexedColor> color = default(Optional<IndexedColor>),
            Optional<object> tag = default(Optional<object>))
        {
            var newCenter = center.HasValue ? center.Value : this.center;
            var newRadius = radius.HasValue ? radius.Value : this.radius;
            var newNormal = normal.HasValue ? normal.Value : this.normal;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (newCenter == this.center &&
                newRadius == this.radius &&
                newNormal == this.normal &&
                newColor == this.Color &&
                newTag == this.Tag)
            {
                return this;
            }

            return new Circle(newCenter, newRadius, newNormal, newColor, newTag);
        }

        internal static Point[] TransformedPoints(Point center, Vector normal, Vector right, double radiusX, double radiusY, params double[] anglesInDegrees)
        {
            var result = new Point[anglesInDegrees.Length];
            var r = right.Normalize();
            var n = normal.Normalize();
            var up = normal.Cross(right).Normalize();
            var trans = Matrix4.FromUnitCircleProjection(n, r, up, center, radiusX, radiusY, 0.0);

            for (int i = 0; i < anglesInDegrees.Length; i++)
            {
                var x = Math.Cos(anglesInDegrees[i] * MathHelper.DegreesToRadians);
                var y = Math.Sin(anglesInDegrees[i] * MathHelper.DegreesToRadians);
                result[i] = trans.Transform(new Point(x, y, 0.0));
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("Circle: center={0}, normal={1}, radius={2}, color={3}", Center, Normal, Radius, Color);
        }
    }
}
