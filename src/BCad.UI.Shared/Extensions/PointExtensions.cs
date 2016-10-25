#if WPF
using UIPoint = System.Windows.Point;
#endif

#if WINDOWS_UWP
using UIPoint = Windows.Foundation.Point;
#endif

namespace BCad.UI.Shared.Extensions
{
    public static class PointExtensions
    {
        public static Point ToPoint(this UIPoint point)
        {
            return new Point(point.X, point.Y, 0.0);
        }
    }
}
