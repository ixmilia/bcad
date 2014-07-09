using System.Drawing;
using System.Windows.Media.Media3D;

namespace BCad.Core.UI.Extensions
{
    public static class PointExtensions
    {
        public static PointF ToPointF(this Point p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }

        public static Point3D ToPoint3D(this Point p)
        {
            return new Point3D(p.X, p.Y, p.Z);
        }
    }
}
