namespace BCad.Entities
{
    public class Vertex
    {
        public Point Location { get; }
        public double Bulge { get; }

        public Vertex(Point location)
            : this(location, 0.0)
        {
        }

        public Vertex(Point location, double bulge)
        {
            Location = location;
            Bulge = bulge;
        }
    }
}
