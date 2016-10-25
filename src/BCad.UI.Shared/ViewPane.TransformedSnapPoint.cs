using BCad.SnapPoints;

namespace BCad.UI.Shared
{
    public partial class ViewPane
    {
        private class TransformedSnapPoint
        {
            public Point WorldPoint;
            public Point ControlPoint;
            public SnapPointKind Kind;

            public TransformedSnapPoint(Point worldPoint, Point controlPoint, SnapPointKind kind)
            {
                WorldPoint = worldPoint;
                ControlPoint = controlPoint;
                Kind = kind;
            }
        }
    }
}
