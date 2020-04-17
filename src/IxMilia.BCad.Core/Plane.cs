namespace IxMilia.BCad
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

        public static Plane From3Points(Point a, Point b, Point c)
        {
            var ab = a - b;
            var cb = c - b;
            var normal = ab.Cross(cb).Normalize();

            // use a positive normal when possible
            if (normal == Vector.XAxis * -1.0)
                normal = Vector.XAxis;
            else if (normal == Vector.YAxis * -1.0)
                normal = Vector.YAxis;
            else if (normal == Vector.ZAxis * -1.0)
                normal = Vector.ZAxis;

            return new Plane(a, normal);
        }
    }
}
