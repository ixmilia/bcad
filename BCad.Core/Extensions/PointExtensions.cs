using System.Windows.Media.Media3D;

namespace BCad
{
    public static class PointExtensions
    {
        public static System.Drawing.Point ToPoint(this Point3D vector)
        {
            return new System.Drawing.Point((int)vector.X, (int)vector.Y);
        }
    }
}
