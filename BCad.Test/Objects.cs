using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.Test
{
    public static class Objects
    {
        public static Line Line()
        {
            return new Line(Point.Origin, Point.Origin, Color.Auto);
        }
    }
}
