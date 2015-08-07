using BCad.Entities;

namespace BCad.Test
{
    public static class Entities
    {
        public static Line Line()
        {
            return new Line(Point.Origin, Point.Origin, null);
        }
    }
}
