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
    }
}
