using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public class Arc : IObject, IPrimitive
    {
        public Point Center { get; private set; }

        public Vector Normal { get; private set; }

        public double Radius { get; private set; }

        public double StartAngle { get; private set; }

        public double EndAngle { get; private set; }

        public Color Color { get; private set; }

        private readonly double startAngleRadians;

        private readonly double endAngleRadians;

        public Arc(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
            Normal = normal;
            Color = color;

            // shortcut values
            startAngleRadians = startAngle * Math.PI / 180.0;
            endAngleRadians = endAngle * Math.PI / 180.0;
        }

        public Point EndPoint1
        {
            get
            {
                return PointFromRadians(startAngleRadians);
            }
        }

        public Point EndPoint2
        {
            get
            {
                return PointFromRadians(endAngleRadians);
            }
        }

        public Point MidPoint
        {
            get
            {
                double angleBetween = EndAngle - StartAngle;
                if (angleBetween < 0.0) angleBetween += 360.0;
                double angle = StartAngle + (angleBetween / 2.0);
                double aRad = angle * Math.PI / 180.0;
                return PointFromRadians(aRad);
            }
        }

        private Point PointFromRadians(double rad)
        {
            Point p = Point.Origin;
            if (Normal == Vector.ZAxis)
                p = new Point(Center.X + Math.Cos(rad) * Radius, Center.Y + Math.Sin(rad) * Radius, Center.Z);
            else if (Normal == Vector.YAxis)
                p = new Point(Center.X + Math.Cos(rad) * Radius, Center.Y, Center.Z + Math.Sin(rad) * Radius);
            else if (Normal == Vector.XAxis)
                p = new Point(Center.X, Center.Y + Math.Cos(rad) * Radius, Center.Z + Math.Sin(rad) * Radius);
            else
                Debug.Fail("Non-axis normal not supported");
            return p;
        }

        public IEnumerable<IPrimitive> GetPrimitives()
        {
            yield return this;
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new CenterPoint(Center);
            yield return new EndPoint(EndPoint1);
            yield return new EndPoint(EndPoint2);
            yield return new MidPoint(MidPoint);
        }

        public Arc Update(Point center = null, double? radius = null, double? startAngle = null, double? endAngle = null, Vector normal = null, Color color = null)
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
