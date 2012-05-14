namespace BCad.Primitives
{
    public interface IPrimitive
    {
        Color Color { get; }
        PrimitiveKind Kind { get; }
    }
}
