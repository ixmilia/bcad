using System;
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
        private readonly PrimitiveEllipse primitive;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Center { get { return center; } }

        public Vector MajorAxis { get { return majorAxis; } }

        public Vector Normal { get { return normal; } }

        public double MinorAxisRatio { get { return minorAxisRatio; } }

        public double StartAngle { get { return startAngle; } }

        public double EndAngle { get { return endAngle; } }

        public Matrix4 FromUnitCircle { get { return primitive.FromUnitCircle; } }

        public Ellipse(Point center, Vector majorAxis, double minorAxisRatio, double startAngle, double endAngle, Vector normal, CadColor? color, object tag = null)
            : base(color, tag)
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

            this.primitive = new PrimitiveEllipse(Center, MajorAxis, Normal, MinorAxisRatio, StartAngle, EndAngle, Color);
            this.primitives = new IPrimitive[] { this.primitive };
            var snaps = new List<SnapPoint>();
            snaps.Add(new CenterPoint(Center));
            if (this.startAngle == 0.0 && this.endAngle == 360.0)
            {
                // treat it like a circle
                snaps.Add(new QuadrantPoint(quadrant1));
                snaps.Add(new QuadrantPoint(quadrant2));
                snaps.Add(new QuadrantPoint(quadrant3));
                snaps.Add(new QuadrantPoint(quadrant4));
            }
            else
            {
                // treat it like an arc
                snaps.Add(new EndPoint(endPoint1));
                snaps.Add(new EndPoint(endPoint2));
                snaps.Add(new MidPoint(midPoint));
            }

            if (MinorAxisRatio != 1.0)
            {
                // a true ellipse with two distinct foci
                var majorNormalized = majorAxis.Normalize();
                var minorLength = majorLength * minorAxisRatio;
                var focusDistance = Math.Sqrt((majorLength * majorLength) - (minorLength * minorLength));
                var focus1 = (Point)((majorNormalized * focusDistance) + Center);
                var focus2 = (Point)((majorNormalized * -focusDistance) + Center);
                snaps.Add(new FocusPoint(focus1));
                snaps.Add(new FocusPoint(focus2));
            }

            this.snapPoints = snaps.ToArray();
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

        public Ellipse Update(
            Optional<Point> center = default(Optional<Point>),
            Optional<Vector> majorAxis = default(Optional<Vector>),
            Optional<double> minorAxisRatio = default(Optional<double>),
            Optional<double> startAngle = default(Optional<double>),
            Optional<double> endAngle = default(Optional<double>),
            Optional<Vector> normal = default(Optional<Vector>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newCenter = center.HasValue ? center.Value : this.center;
            var newMajorAxis = majorAxis.HasValue ? majorAxis.Value : this.majorAxis;
            var newMinorAxisRatio = minorAxisRatio.HasValue ? minorAxisRatio.Value : this.minorAxisRatio;
            var newStartAngle = startAngle.HasValue ? startAngle.Value : this.startAngle;
            var newEndAngle = endAngle.HasValue ? endAngle.Value : this.endAngle;
            var newNormal = normal.HasValue ? normal.Value : this.normal;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (newCenter == this.center &&
                newMajorAxis == this.majorAxis &&
                newMinorAxisRatio == this.minorAxisRatio &&
                newStartAngle == this.startAngle &&
                newEndAngle == this.endAngle &&
                newNormal == this.normal &&
                newColor == this.Color &&
                newTag == this.Tag)
            {
                return this;
            }

            return new Ellipse(newCenter, newMajorAxis, newMinorAxisRatio, newStartAngle, newEndAngle, newNormal, newColor, newTag);
        }

        public override string ToString()
        {
            return string.Format("Ellipse: center={0}, major-axis={1}, normal={2}, minor={3}, start/end={4}/{5}, color={6}", Center, MajorAxis, Normal, MinorAxisRatio, StartAngle, EndAngle, Color);
        }
    }
}
