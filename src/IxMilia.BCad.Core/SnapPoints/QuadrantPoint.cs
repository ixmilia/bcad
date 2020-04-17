namespace IxMilia.BCad.SnapPoints
{
    public class QuadrantPoint : SnapPoint
    {
        public QuadrantPoint(Point p)
            : base(p)
        {
        }

        public override SnapPointKind Kind
        {
            get { return SnapPointKind.Quadrant; }
        }
    }
}
