using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Helpers
{
    public static class MathHelper
    {
        public static bool Between(double min, double max, double value)
        {
            return value >= min && value <= max;
        }
    }
}
