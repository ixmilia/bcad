using System;

namespace BCad.Helpers
{
    public static class MathHelper
    {
        public const int Precision = 12;
        public const double PI = 3.1415926535898;
        public const double RadiansToDegrees = 180.0 / MathHelper.PI;
        public const double DegreesToRadians = MathHelper.PI / 180.0;
        public const double TwoPI = MathHelper.PI * 2.0;
        public const double Epsilon = 0.000000000001;

        public static bool Between(double a, double b, double value)
        {
            var min = Math.Min(a, b) - Epsilon;
            var max = Math.Max(a, b) + Epsilon;
            return value >= min && value <= max;
        }

        public static bool BetweenNarrow(double a, double b, double value)
        {
            var min = Math.Min(a, b) + Epsilon;
            var max = Math.Max(a, b) - Epsilon;
            return value >= min && value <= max;
        }

        public static double CorrectAngleDegrees(this double angle)
        {
            while (angle < 0.0)
                angle += 360.0;
            while (angle >= 360.0)
                angle -= 360.0;
            return angle;
        }
    }
}
