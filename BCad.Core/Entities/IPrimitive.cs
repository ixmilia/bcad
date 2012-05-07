namespace BCad.Entities
{
    public interface IPrimitive
    {
        Color Color { get; }
        PrimitiveKind Kind { get; }
    }
}
