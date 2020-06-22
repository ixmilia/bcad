using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Display
{
    public struct TransformedSnapPoint
    {
        public Point WorldPoint { get; }
        public Point ControlPoint { get; }
        public SnapPointKind Kind { get; }

        public TransformedSnapPoint(Point worldPoint, Point controlPoint, SnapPointKind kind)
        {
            WorldPoint = worldPoint;
            ControlPoint = controlPoint;
            Kind = kind;
        }
    }
}
