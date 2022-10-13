using System;
using System.Collections.Generic;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Circle : Entity
    {
        private readonly PrimitiveEllipse _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public Point Center => _primitive.Center;

        public Vector Normal => _primitive.Normal;

        public double Radius { get; }

        public Matrix4 FromUnitCircle => _primitive.FromUnitCircle;

        public double Thickness => _primitive.Thickness;

        public override EntityKind Kind => EntityKind.Circle;

        public override BoundingBox BoundingBox { get; }

        public Circle(Point center, double radius, Vector normal, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null, double thickness = default(double))
            : this(new PrimitiveEllipse(center, radius, normal, color, thickness), lineTypeSpecification, tag)
        {
        }

        public Circle(PrimitiveEllipse ellipse, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(ellipse.Color, lineTypeSpecification, tag)
        {
            if (!ellipse.IsCircle)
            {
                throw new ArgumentException($"{nameof(PrimitiveEllipse)} was not a circle");
            }

            _primitive = ellipse;
            _primitives = new[] { _primitive };
            Radius = ellipse.MajorAxis.Length;

            var right = Vector.RightVectorFromNormal(Normal);
            var points = TransformedPoints(Center, Normal, right, Radius, Radius, 0, 90, 180, 270);
            var quadrant1 = points[0];
            var quadrant2 = points[1];
            var quadrant3 = points[2];
            var quadrant4 = points[3];

            _snapPoints = new SnapPoint[]
            {
                new CenterPoint(Center),
                new QuadrantPoint(quadrant1),
                new QuadrantPoint(quadrant2),
                new QuadrantPoint(quadrant3),
                new QuadrantPoint(quadrant4)
            };
            BoundingBox = BoundingBox.FromPoints(points);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Circle Update(
            Optional<Point> center = default,
            Optional<double> radius = default,
            Optional<Vector> normal = default,
            Optional<double> thickness = default,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newCenter = center.HasValue ? center.Value : Center;
            var newRadius = radius.HasValue ? radius.Value : Radius;
            var newNormal = normal.HasValue ? normal.Value : Normal;
            var newThickness = thickness.HasValue ? thickness.Value : Thickness;
            var newColor = color.HasValue ? color.Value : Color;
            var newLineTypeSpecification = lineTypeSpecification.HasValue ? lineTypeSpecification.Value : LineTypeSpecification;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newCenter == Center &&
                newRadius == Radius &&
                newNormal == Normal &&
                newThickness == Thickness &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new Circle(newCenter, newRadius, newNormal, newColor, newLineTypeSpecification, newTag, newThickness);
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
