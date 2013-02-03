using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Arc : Entity
    {
        private readonly Point center;
        private readonly Vector normal;
        private readonly double radius;
        private readonly double startAngle;
        private readonly double endAngle;
        private readonly Color color;
        private readonly Point endPoint1;
        private readonly Point endPoint2;
        private readonly Point midPoint;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Center { get { return center; } }

        public Vector Normal { get { return normal; } }

        public double Radius { get { return radius; } }

        public double StartAngle { get { return startAngle; } }

        public double EndAngle { get { return endAngle; } }

        public Color Color { get { return color; } }

        public Arc(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color)
        {
            this.center = center;
            this.radius = radius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.normal = normal;
            this.color = color;

            var right = Vector.RightVectorFromNormal(this.normal);
            var midAngle = (startAngle + endAngle) / 2.0;
            if (startAngle > endAngle)
                midAngle -= 180.0;
            var points = Circle.TransformedPoints(this.center, this.normal, right, this.radius, this.radius, startAngle, endAngle, midAngle);
            this.endPoint1 = points[0];
            this.endPoint2 = points[1];
            this.midPoint = points[2];

            this.primitives = new[] { new PrimitiveEllipse(Center, Radius, StartAngle, EndAngle, Normal, Color) };
            this.snapPoints = new SnapPoint[]
            {
                new CenterPoint(Center),
                new EndPoint(EndPoint1),
                new EndPoint(EndPoint2),
                new MidPoint(MidPoint)
            };
            this.boundingBox = BoundingBox.FromPoints(Circle.TransformedPoints(this.center, this.normal, right, this.radius, this.radius, 0, 90, 180, 270));
        }

        public Point EndPoint1 { get { return this.endPoint1; } }

        public Point EndPoint2 { get { return this.endPoint2; } }

        public Point MidPoint { get { return this.midPoint; } }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override EntityKind Kind { get { return EntityKind.Arc; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Arc Update(Point center = null, double? radius = null, double? startAngle = null, double? endAngle = null, Vector normal = null, Color? color = null)
        {
            return new Arc(
                center ?? this.Center,
                radius ?? this.Radius,
                startAngle ?? this.StartAngle,
                endAngle ?? this.EndAngle,
                normal ?? this.Normal,
                color ?? this.Color);
        }
    }
}
