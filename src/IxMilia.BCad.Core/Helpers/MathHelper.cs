using System;

namespace IxMilia.BCad.Helpers
{
    public static class MathHelper
    {
        public const int Precision = 12;
        public const double PI = 3.1415926535898;
        public const double OneEighty = 180.0;
        public const double ThreeSixty = 360.0;
        public const double RadiansToDegrees = OneEighty / MathHelper.PI;
        public const double DegreesToRadians = MathHelper.PI / OneEighty;
        public const double TwoPI = MathHelper.PI * 2.0;
        public const double Epsilon = 1.0E-12;
        public const double BezierEpsilon = 1.0 / 1024.0;
        public const double SqrtThreeHalves = 1.2247448713916;

        public readonly static double[] SIN;
        public readonly static double[] COS;
        public const int DefaultPixelBuffer = 20;

        static MathHelper()
        {
            SIN = new double[360];
            COS = new double[360];
            double rad;
            for (int i = 0; i < 360; i++)
            {
                rad = i * DegreesToRadians;
                SIN[i] = Math.Sin(rad);
                COS[i] = Math.Cos(rad);
            }
        }

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

        public static double CorrectAngleRadians(this double angle)
        {
            while (angle < 0.0)
                angle += TwoPI;
            while (angle >= TwoPI)
                angle -= TwoPI;
            return angle;
        }

        public static Tuple<double, double> CorrectAnglesDegrees(double startAngle, double endAngle)
        {
            var newStartAngle = startAngle.CorrectAngleDegrees();
            var newEndAngle = endAngle;
            while (newEndAngle < newStartAngle)
            {
                newEndAngle += ThreeSixty;
            }

            return Tuple.Create(newStartAngle, newEndAngle);
        }

        public static Tuple<double, double> EnsureMinorAngleDegrees(double startAngle, double endAngle)
        {
            var correctedAngles = CorrectAnglesDegrees(startAngle, endAngle);
            var newStartAngle = correctedAngles.Item1;
            var newEndAngle = correctedAngles.Item2;
            var containedAngle = newEndAngle - newStartAngle;
            if (containedAngle >= OneEighty)
            {
                var temp = newStartAngle;
                newStartAngle = newEndAngle;
                newEndAngle = temp;

                // while swapping, we may have to turn 360 into 0
                while (newStartAngle > newEndAngle)
                {
                    newStartAngle -= ThreeSixty;
                }
            }

            return Tuple.Create(newStartAngle.CorrectAngleDegrees(), newEndAngle.CorrectAngleDegrees());
        }

        public static bool CloseTo(double expected, double actual, double epsilon = Epsilon)
        {
            return Between(expected - epsilon, expected + epsilon, actual);
        }

        public static bool CloseTo(Vector expected, Vector actual, double epsilon = Epsilon)
        {
            return CloseTo(expected.X, actual.X, epsilon)
                && CloseTo(expected.Y, actual.Y, epsilon)
                && CloseTo(expected.Z, actual.Z, epsilon);
        }

        public static bool CloseTo(Matrix4 expected, Matrix4 actual, double epsilon = Epsilon)
        {
            return CloseTo(expected.M11, actual.M11, epsilon)
                && CloseTo(expected.M12, actual.M12, epsilon)
                && CloseTo(expected.M13, actual.M13, epsilon)
                && CloseTo(expected.M14, actual.M14, epsilon)
                && CloseTo(expected.M21, actual.M21, epsilon)
                && CloseTo(expected.M22, actual.M22, epsilon)
                && CloseTo(expected.M23, actual.M23, epsilon)
                && CloseTo(expected.M24, actual.M24, epsilon)
                && CloseTo(expected.M31, actual.M31, epsilon)
                && CloseTo(expected.M32, actual.M32, epsilon)
                && CloseTo(expected.M33, actual.M33, epsilon)
                && CloseTo(expected.M34, actual.M34, epsilon)
                && CloseTo(expected.M41, actual.M41, epsilon)
                && CloseTo(expected.M42, actual.M42, epsilon)
                && CloseTo(expected.M43, actual.M43, epsilon)
                && CloseTo(expected.M44, actual.M44, epsilon);
        }

        public static double CubeRoot(double v)
        {
            return v < 0.0
                ? -Math.Pow(-v, 1.0 / 3.0)
                : Math.Pow(v, 1.0 / 3.0);
        }
    }
}
