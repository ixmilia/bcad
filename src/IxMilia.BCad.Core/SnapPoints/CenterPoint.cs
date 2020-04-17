namespace IxMilia.BCad.SnapPoints
{
    public class CenterPoint : SnapPoint
    {
        public CenterPoint(Point p)
            : base(p)
        {
        }

        public override SnapPointKind Kind
        {
            get { return SnapPointKind.Center; }
        }
    }
}
