using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Extensions
{
    public static class VectorExtensions
    {
        public static bool IsCloseToZeroVector(this Vector v)
        {
            return MathHelper.CloseTo(v.X, 0.0)
                && MathHelper.CloseTo(v.Y, 0.0)
                && MathHelper.CloseTo(v.Z, 0.0);
        }
    }
}
