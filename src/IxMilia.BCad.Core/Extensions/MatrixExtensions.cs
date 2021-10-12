using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Extensions
{
    public static class MatrixExtensions
    {
        public static PrimitiveLine Transform(this Matrix4 matrix, PrimitiveLine line)
        {
            return new PrimitiveLine(matrix.Transform(line.P1), matrix.Transform(line.P2), line.Color);
        }
    }
}
