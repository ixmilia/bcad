using SharpDX;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Point ToPoint(this System.Windows.Point point)
        {
            return new Point(point.X, point.Y, 0);
        }
    }
}
