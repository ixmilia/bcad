using System.Drawing;

namespace BCad.Core.UI.Extensions
{
    public static class PointExtensions
    {
        public static PointF ToPointF(this Point p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }
    }
}
