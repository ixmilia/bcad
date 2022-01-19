using System;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Extensions
{
    public static class QuantizeSettingsExtensions
    {
        public static double QuantizeValue(double value, double quantum)
        {
            return (double)((int)((value / quantum) + (0.5 * Math.Sign(value)))) * quantum;
        }

        public static double QuantizeAngle(this QuantizeSettings settings, double d)
        {
            return QuantizeValue(d, settings.AngleQuantum);
        }

        public static double QuantizeDistance(this QuantizeSettings settings, double d)
        {
            return QuantizeValue(d, settings.DistanceQuantum);
        }

        public static Point Quantize(this QuantizeSettings settings, Point p)
        {
            var x = settings.QuantizeDistance(p.X);
            var y = settings.QuantizeDistance(p.Y);
            var z = settings.QuantizeDistance(p.Z);
            return new Point(x, y, z);
        }

        public static Vector Quantize(this QuantizeSettings settings, Vector v)
        {
            var x = settings.QuantizeDistance(v.X);
            var y = settings.QuantizeDistance(v.Y);
            var z = settings.QuantizeDistance(v.Z);
            return new Vector(x, y, z);
        }
    }
}
