namespace BCad.Iges
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
    }
}
