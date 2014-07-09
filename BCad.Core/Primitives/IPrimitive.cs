namespace BCad.Primitives
{
    public interface IPrimitive
    {
        IndexedColor Color { get; }
        PrimitiveKind Kind { get; }
    }
}
