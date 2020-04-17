namespace IxMilia.BCad.SnapPoints
{
    public abstract class SnapPoint
    {
        public Point Point { get; protected set; }

        public abstract SnapPointKind Kind { get; }

        public SnapPoint(Point p)
        {
            Point = p;
        }
    }
}
