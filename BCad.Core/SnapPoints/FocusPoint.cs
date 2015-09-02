namespace BCad.SnapPoints
{
    public class FocusPoint : SnapPoint
    {
        public FocusPoint(Point p)
            : base(p)
        {
        }

        public override SnapPointKind Kind
        {
            get { return SnapPointKind.Focus; }
        }
    }
}
