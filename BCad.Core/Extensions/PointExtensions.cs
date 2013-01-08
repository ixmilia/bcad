using BCad.Helpers;
using System.Windows.Media.Media3D;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Point Transform(this Point point, Matrix3D matrix)
        {
            return matrix.Transform(point);
        }

        public static System.Drawing.Point ToDrawingPoint(Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static bool CloseTo(this Point expected, Point actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }

        public static bool CloseTo(this Vector expected, Vector actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }
    }
}
