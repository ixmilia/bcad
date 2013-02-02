using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Stl
{
    public struct StlVertex
    {
        public double X;
        public double Y;
        public double Z;

        public StlVertex(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
