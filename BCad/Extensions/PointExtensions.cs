using SharpDX;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }
    }
}
