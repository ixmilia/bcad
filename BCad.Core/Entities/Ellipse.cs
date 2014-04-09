using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Ellipse : Entity
    {
        private const string MajorAxisText = "MajorAxis";
        private const string MinorAxisRatioText = "MinorAxisRatio";
        private readonly Point center;
        private readonly Vector majorAxis;
        private readonly Vector normal;
        private readonly double minorAxisRatio;
        private readonly double startAngle;
        private readonly double endAngle;
        private readonly Point quadrant1;
        private readonly Point quadrant2;
        private readonly Point quadrant3;
        private readonly Point quadrant4;
        private readonly Point endPoint1;
        private readonly Point endPoint2;
        private readonly Point midPoint;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Center { get { return center; } }

        public Vector MajorAxis { get { return majorAxis; } }

        public Vector Normal { get { return normal; } }

        public double MinorAxisRatio { get { return minorAxisRatio; } }

        public double StartAngle { get { return startAngle; } }

        public double EndAngle { get { return endAngle; } }

        public Ellipse(Point center, Vector majorAxis, double minorAxisRatio, double startAngle, double endAngle, Vector normal, IndexedColor color)
            : base(color)
        {
            this.center = center;
            this.majorAxis = majorAxis;
            this.minorAxisRatio = minorAxisRatio;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.normal = normal;

            var majorLength = this.majorAxis.Length;
            var points = Circle.TransformedPoints(this.center, this.normal, this.majorAxis, majorLength, majorLength * minorAxisRatio, 0, 90, 180, 270, startAngle, endAngle, (startAngle + endAngle) / 2.0);
            quadrant1 = points[0];
            quadrant2 = points[1];
            quadrant3 = points[2];
            quadrant4 = points[3];
            endPoint1 = points[4];
            endPoint2 = points[5];
            midPoint = points[6];

            this.primitives = new[] { new PrimitiveEllipse(Center, MajorAxis, Normal, MinorAxisRatio, StartAngle, EndAngle, Color) };
            if (this.startAngle == 0.0 && this.endAngle == 360.0)
            {
                // treat it like a circle
                this.snapPoints = new SnapPoint[]
                {
                    new CenterPoint(Center),
                    new QuadrantPoint(quadrant1),
                    new QuadrantPoint(quadrant2),
                    new QuadrantPoint(quadrant3),
                    new QuadrantPoint(quadrant4)
                };
            }
            else
            {
                // treat it like an arc
                this.snapPoints = new SnapPoint[]
                {
                    new CenterPoint(Center),
                    new EndPoint(endPoint1),
                    new EndPoint(endPoint2),
                    new MidPoint(midPoint)
                };
            }
            this.boundingBox = BoundingBox.FromPoints(quadrant1, quadrant2, quadrant3, quadrant4);
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
                case MajorAxisText:
                    return MajorAxis;
                case MinorAxisRatioText:
                    return MinorAxisRatio;
                case StartAngleText:
                    return StartAngle;
                case EndAngleText:
                    return EndAngle;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Ellipse; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Ellipse Update(Point center = null, Vector majorAxis = null, double? minorAxisRatio = null, double? startAngle = null, double? endAngle = null, Vector normal = null, IndexedColor? color = null)
        {
            return new Ellipse(
                center ?? this.center,
                majorAxis ?? this.majorAxis,
                minorAxisRatio ?? this.minorAxisRatio,
                startAngle ?? this.startAngle,
                endAngle ?? this.endAngle,
                normal ?? this.normal,
                color ?? Color)
            {
                Tag = this.Tag
            };
        }

        public override string ToString()
        {
            return string.Format("Ellipse: center={0}, major-axis={1}, normal={2}, minor={3}, start/end={4}/{5}, color={6}", Center, MajorAxis, Normal, MinorAxisRatio, StartAngle, EndAngle, Color);
        }
    }
}
