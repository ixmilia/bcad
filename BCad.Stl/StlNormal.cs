using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Stl
{
    public struct StlNormal
    {
        public double X;
        public double Y;
        public double Z;

        public StlNormal(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public StlNormal Normalize()
        {
            var length = Math.Sqrt(X * X + Y * Y + Z * Z);
            if (length == 0.0)
                return new StlNormal();
            return new StlNormal(X / length, Y / length, Z / length);
        }
    }
}
