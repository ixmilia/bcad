using System;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
        private const double ThreeSixty = 360.0;
        public const int MaxSegmentCount = 360;

        public static Point[] GetProjectedVerticies(this PrimitiveEllipse ellipse, Matrix4 projectionMatrix, int maxSegmentCount = MaxSegmentCount)
        {
            var startAngleDeg = ellipse.StartAngle;
            var endAngleDeg = ellipse.EndAngle;
            if (endAngleDeg < startAngleDeg)
                endAngleDeg += ThreeSixty;
            var startAngleRad = startAngleDeg * MathHelper.DegreesToRadians;
            var endAngleRad = endAngleDeg * MathHelper.DegreesToRadians;
            if (endAngleRad < startAngleRad)
                endAngleRad += MathHelper.TwoPI;
            var vertexCount = (int)Math.Ceiling((endAngleDeg - startAngleDeg) / ThreeSixty * maxSegmentCount);
            var verticies = new Point[vertexCount + 1];
            var angleDelta = ThreeSixty / maxSegmentCount * MathHelper.DegreesToRadians;
            var trans = projectionMatrix * ellipse.FromUnitCircleProjection();
            double angle;
            int i;
            for (angle = startAngleRad, i = 0; i < vertexCount; angle += angleDelta, i++)
            {
                verticies[i] = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
            }

            verticies[i] = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));

            return verticies;
        }
    }
}
