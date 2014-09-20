namespace IxMilia.Iges
{
    public class IgesPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public IgesPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static IgesPoint Origin
        {
            get
            {
                return new IgesPoint(0.0, 0.0, 0.0);
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }
    }

    public class IgesVector : IgesPoint
    {
        public IgesVector(double x, double y, double z)
            : base(x, y, z)
        {
        }

        public static IgesVector Zero
        {
            get { return new IgesVector(0.0, 0.0, 0.0); }
        }

        public static IgesVector ZAxis
        {
            get { return new IgesVector(0.0, 0.0, 1.0); }
        }
    }
}
