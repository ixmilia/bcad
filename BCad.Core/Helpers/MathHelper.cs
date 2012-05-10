using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Helpers
{
    public static class MathHelper
    {
        public const double RadiansToDegrees = 180.0 / Math.PI;

        public const double DegreesToRadians = Math.PI / 180.0;

        public const double TwoPI = Math.PI * 2.0;

        public static bool Between(double min, double max, double value)
        {
            return value >= min && value <= max;
        }
    }
}
