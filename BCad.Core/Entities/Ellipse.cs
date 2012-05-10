using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.SnapPoints;
using BCad.Helpers;

namespace BCad.Entities
{
    public class Ellipse : Entity, IPrimitive
    {
        private readonly Point center;
        private readonly Vector majorAxis;
        private readonly Vector normal;
        private readonly double minorAxisRatio;
        private readonly double startAngle;
        private readonly double endAngle;
        private readonly Color color;
        private readonly Point quadrant1;
        private readonly Point quadrant2;
        private readonly Point quadrant3;
        private readonly Point quadrant4;
        private readonly Point endPoint1;
        private readonly Point endPoint2;
        private readonly Point midPoint;

        public Point Center { get { return center; } }

        public Vector MajorAxis { get { return majorAxis; } }

        public Vector Normal { get { return normal; } }

        public double MinorAxisRatio { get { return minorAxisRatio; } }

        public double StartAngle { get { return startAngle; } }

        public double EndAngle { get { return endAngle; } }

        public Color Color { get { return color; } }

        public Ellipse(Point center, Vector majorAxis, double minorAxisRatio, double startAngle, double endAngle, Vector normal, Color color)
        {
            this.center = center;
            this.majorAxis = majorAxis;
            this.minorAxisRatio = minorAxisRatio;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.normal = normal;
            this.color = color;

            var majorLength = this.majorAxis.Length;
            var points = Circle.TransformedPoints(
                this.center,
                this.normal,
                majorLength,
                majorLength * minorAxisRatio,
                0, 90, 180, 270, startAngle, endAngle, (startAngle + endAngle) / 2.0);
            quadrant1 = points[0];
            quadrant2 = points[1];
            quadrant3 = points[2];
            quadrant4 = points[3];
            endPoint1 = points[4];
            endPoint2 = points[5];
            midPoint = points[6];
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return new[] { this };
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            if (this.startAngle == 0.0 && this.endAngle == 360.0)
            {
                // treat it like a circle
                return new SnapPoint[]
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
                return new SnapPoint[]
                {
                    new CenterPoint(Center),
                    new EndPoint(endPoint1),
                    new EndPoint(endPoint2),
                    new MidPoint(midPoint)
                };
            }
        }

        public PrimitiveKind Kind
        {
            get { return PrimitiveKind.Ellipse; }
        }

        public Ellipse Update(Point center = null, Vector majorAxis = null, double? minorAxisRatio = null, double? startAngle = null, double? endAngle = null, Vector normal = null, Color? color = null)
        {
            return new Ellipse(
                center ?? this.center,
                majorAxis ?? this.majorAxis,
                minorAxisRatio ?? this.minorAxisRatio,
                startAngle ?? this.startAngle,
                endAngle ?? this.endAngle,
                normal ?? this.normal,
                color ?? this.color);
        }
    }
}
