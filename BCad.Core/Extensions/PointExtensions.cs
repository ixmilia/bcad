using BCad.Helpers;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static bool CloseTo(this Point expected, Point actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }

        public static bool CloseTo(this Vector expected, Vector actual)
        {
            return MathHelper.CloseTo(expected.X, actual.X)
                && MathHelper.CloseTo(expected.Y, actual.Y)
                && MathHelper.CloseTo(expected.Z, actual.Z);
        }
    }
}
