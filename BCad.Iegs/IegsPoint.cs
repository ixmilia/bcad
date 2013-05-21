namespace BCad.Iegs
{
    public class IegsPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public IegsPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static IegsPoint Origin
        {
            get
            {
                return new IegsPoint(0.0, 0.0, 0.0);
            }
        }
    }
}
