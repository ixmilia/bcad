using System;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
        public static Point[] GetProjectedVerticies(this PrimitiveEllipse ellipse, Matrix4 projectionMatrix)
        {
            var startAngle = ellipse.StartAngle * MathHelper.DegreesToRadians;
            var endAngle = ellipse.EndAngle * MathHelper.DegreesToRadians;
            if (endAngle < startAngle)
                endAngle += MathHelper.TwoPI;
            var vertexCount = (int)Math.Ceiling(ellipse.EndAngle - ellipse.StartAngle);
            var verticies = new Point[vertexCount + 1];
            var angleDelta = 1.0 * MathHelper.DegreesToRadians;
            var trans = projectionMatrix * ellipse.FromUnitCircleProjection();
            double angle;
            int i;
            for (angle = startAngle, i = 0; angle < endAngle; angle += angleDelta, i++)
            {
                verticies[i] = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
            }

            if (angle < endAngle)
            {
                verticies[i] = trans.Transform(new Point(Math.Cos(endAngle), Math.Sin(endAngle), 0.0));
            }

            return verticies;
        }
    }
}
