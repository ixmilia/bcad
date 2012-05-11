using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using BCad.SnapPoints;
using BCad.Helpers;

namespace BCad.Entities
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

            var right = Vector.RightVectorFromNormal(this.normal);
            var points = TransformedPoints(this.center, this.normal, right, this.radius, this.radius, 0, 90, 180, 270);
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

        internal static Point[] TransformedPoints(Point center, Vector normal, Vector right, double radiusX, double radiusY, params double[] anglesInDegrees)
        {
            var result = new Point[anglesInDegrees.Length];
            var r = right.Normalize();
            var n = normal.Normalize();
            var up = normal.Cross(right).Normalize();
            var trans = Matrix3D.Identity;
            trans.M11 = r.X;
            trans.M12 = r.Y;
            trans.M13 = r.Z;
            trans.M21 = up.X;
            trans.M22 = up.Y;
            trans.M23 = up.Z;
            trans.M31 = n.X;
            trans.M32 = n.Y;
            trans.M33 = n.Z;
            trans.OffsetX = center.X;
            trans.OffsetY = center.Y;
            trans.OffsetZ = center.Z;

            for (int i = 0; i < anglesInDegrees.Length; i++)
            {
                var x = Math.Cos(anglesInDegrees[i] * MathHelper.DegreesToRadians);
                var y = Math.Sin(anglesInDegrees[i] * MathHelper.DegreesToRadians);
                result[i] = new Point(trans.Transform(new Point3D(x * radiusX, y * radiusY, 0.0)));
            }

            return result;
        }
    }
}
