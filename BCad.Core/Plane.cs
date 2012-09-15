namespace BCad
{
    public class Plane
    {
        public Point Point { get; set; }

        public Vector Normal { get; set; }

        public Plane(Point point, Vector normal)
        {
            this.Point = point;
            this.Normal = normal;
        }

        public static Plane XY
        {
            get { return new Plane(Point.Origin, Vector.ZAxis); }
        }
    }
}
