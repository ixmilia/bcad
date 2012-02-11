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

        public Layer Layer { get; private set; }

        private int hashCode;

        public Vector Normal
        {
            get { return normal; }
            set
            {
                value.Normalize();
                if (value != Vector.XAxis && value != Vector.YAxis && value != Vector.ZAxis)
                    throw new Exception("The normal vector must be a major axis.");
                normal = value;
            }
        }

        public double Radius { get; set; }

        public double StartAngle
        {
            get { return startAngle; }
            set
            {
                startAngle = value;
                startAngleRadians = startAngle * Math.PI / 180.0;
            }
        }

        public double EndAngle
        {
            get { return endAngle; }
            set
            {
                endAngle = value;
                endAngleRadians = endAngle * Math.PI / 180.0;
            }
        }

        public Color Color { get; set; }

        public Arc(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color)
            : this(center, radius, startAngle, endAngle, normal, color, null)
        {
        }

        public Arc(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color, Layer layer)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
            Normal = normal;
            Color = color;
            Layer = layer;
            hashCode = Center.GetHashCode() ^ Normal.GetHashCode() ^ Radius.GetHashCode() ^ StartAngle.GetHashCode() ^ EndAngle.GetHashCode();
        }

        private Vector normal = Vector.ZAxis;
        private double startAngle = 0.0;
        private double endAngle = 0.0;
        private double startAngleRadians = 0.0;
        private double endAngleRadians = 0.0;

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
                double angleBetween = endAngle - startAngle;
                if (angleBetween < 0.0) angleBetween += 360.0;
                double angle = startAngle + (angleBetween / 2.0);
                double aRad = angle * Math.PI / 180.0;
                return PointFromRadians(aRad);
            }
        }

        private Point PointFromRadians(double rad)
        {
            Point p = new Point();
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

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
