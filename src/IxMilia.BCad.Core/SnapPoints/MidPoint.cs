namespace IxMilia.BCad.SnapPoints
{
    public class MidPoint : SnapPoint
    {
        public MidPoint(Point p)
            : base(p)
        {
        }

        public override SnapPointKind Kind
        {
            get { return SnapPointKind.MidPoint; }
        }
    }
}
