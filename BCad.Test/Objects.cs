using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Entities;

namespace BCad.Test
{
    public static class Entities
    {
        public static Line Line()
        {
            return new Line(Point.Origin, Point.Origin, IndexedColor.Auto);
        }
    }
}
