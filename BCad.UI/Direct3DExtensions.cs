using SlimDX;

namespace BCad.UI
{
    public static class Direct3DExtensions
    {
        public static Vector3 ToVector3(this System.Windows.Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, 0.0f);
        }

        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Point ToPoint(this Vector3 point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        public static System.Windows.Point ToWindowsPoint(this Vector3 point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }
    }
}
