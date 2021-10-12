namespace IxMilia.BCad.Entities
{
    public enum EntityKind
    {
        Aggregate = 1 << 0,
        Arc = 1 << 1,
        Circle = 1 << 2,
        Ellipse = 1 << 3,
        Line = 1 << 4,
        Location = 1 << 5,
        Polyline = 1 << 6,
        Text = 1 << 7,
        Spline = 1 << 8,
        All = int.MaxValue
    }
}
