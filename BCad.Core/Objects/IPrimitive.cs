namespace BCad.Objects
{
    public interface IPrimitive
    {
        Color Color { get; }
        PrimitiveKind Kind { get; }
    }
}
