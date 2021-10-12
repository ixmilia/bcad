namespace IxMilia.BCad.Primitives
{
    public enum PrimitiveKind
    {
        Ellipse = 1 << 0,
        Line = 1 << 1,
        Point = 1 << 2,
        Text = 1 << 3,
        Bezier = 1 << 4,
    }
}
