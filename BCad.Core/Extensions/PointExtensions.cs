using System.Windows.Media.Media3D;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Point Transform(this Point point, Matrix3D matrix)
        {
            return new Point(matrix.Transform(point.ToPoint3D()));
        }

        public static Vector Transform(this Vector vector, Matrix3D matrix)
        {
            return ((Point)vector).Transform(matrix);
        }
    }
}
