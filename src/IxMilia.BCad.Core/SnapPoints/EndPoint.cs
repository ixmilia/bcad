namespace IxMilia.BCad.SnapPoints
{
    public class EndPoint : SnapPoint
    {
        public EndPoint(Point p)
            : base(p)
        {
        }

        public override SnapPointKind Kind
        {
            get { return SnapPointKind.EndPoint; }
        }
    }
}
