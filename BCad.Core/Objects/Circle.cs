using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BCad.SnapPoints;
using System.Windows.Media.Media3D;

namespace BCad.Objects
{
    public class Circle : Entity, IPrimitive
    {
        private readonly Point center;
        private readonly Vector normal;
        private readonly double radius;
        private readonly Color color;
        private readonly Point quadrant1;
        private readonly Point quadrant2;
        private readonly Point quadrant3;
        private readonly Point quadrant4;

        public Point Center { get { return center; } }

        public Vector Normal { get { return normal; } }

        public double Radius { get { return radius; } }

        public Color Color { get { return color; } }

        public Circle(Point center, double radius, Vector normal, Color color)
        {
            this.center = center;
            this.radius = radius;
            this.normal = normal;
            this.color = color;

            var points = TransformedPoints(this.center, this.normal, this.radius, 0, 90, 180, 270);
            quadrant1 = points[0];
            quadrant2 = points[1];
            quadrant3 = points[2];
            quadrant4 = points[3];
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return new[] { this };
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return new SnapPoint[]
            {
                new CenterPoint(Center),
                new QuadrantPoint(quadrant1),
                new QuadrantPoint(quadrant2),
                new QuadrantPoint(quadrant3),
                new QuadrantPoint(quadrant4)
            };
        }

        public PrimitiveKind Kind
        {
            get { return PrimitiveKind.Circle; }
        }

        public Circle Update(Point center = null, double? radius = null, Vector normal = null, Color? color = null)
        {
            return new Circle(
                center ?? this.Center,
                radius ?? this.Radius,
                normal ?? this.Normal,
                color ?? this.Color);
        }

        internal static Point[] TransformedPoints(Point center, Vector normal, double radius, params double[] anglesInDegrees)
        {
            var result = new Point[anglesInDegrees.Length];

            var trans = Matrix3D.Identity;
            trans.Scale(new Vector3D(radius, radius, radius));
            trans.Rotate(new Quaternion(new Vector3D(0, 0, 1), -Math.Atan2(normal.Y, normal.X) / Math.PI * 180.0));
            trans.Rotate(new Quaternion(new Vector3D(1, 0, 0), -Math.Atan2(normal.Y, normal.Z) / Math.PI * 180.0));
            trans.Rotate(new Quaternion(new Vector3D(0, 1, 0), Math.Atan2(normal.X, normal.Z) / Math.PI * 180.0));
            trans.Translate(center.ToVector().ToVector3D());

            for (int i = 0; i < anglesInDegrees.Length; i++)
            {
                var x = Math.Cos(anglesInDegrees[i] / 180.0 * Math.PI);
                var y = Math.Sin(anglesInDegrees[i] / 180.0 * Math.PI);
                result[i] = new Point(trans.Transform(new Point3D(x, y, 0.0)));
            }

            return result;
        }
    }
}
