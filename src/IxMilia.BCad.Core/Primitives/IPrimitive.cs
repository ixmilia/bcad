namespace IxMilia.BCad.Primitives
{
    public interface IPrimitive
    {
        CadColor? Color { get; }
        PrimitiveKind Kind { get; }
    }
}
