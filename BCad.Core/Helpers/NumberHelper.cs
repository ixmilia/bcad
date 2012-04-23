using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Helpers
{
    public static class NumberHelper
    {
        public static bool Between(double value, double lower, double upper)
        {
            var min = Math.Min(lower, upper);
            var max = Math.Max(lower, upper);
            return value >= min && value <= max;
        }
    }
}
