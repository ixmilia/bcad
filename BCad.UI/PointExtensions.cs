namespace BCad.UI
{
    public static class PointExtensions
    {
        public static Point ToPoint(this System.Windows.Point p)
        {
            return new Point(p.X, p.Y, 0.0);
        }
    }
}
