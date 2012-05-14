namespace BCad.Primitives
{
    public class PrimitiveEllipse : IPrimitive
    {
        public Point Center { get; private set; }
        public Vector MajorAxis { get; private set; }
        public Vector Normal { get; private set; }
        public double MinorAxisRatio { get; private set; }
        public double StartAngle { get; private set; }
        public double EndAngle { get; private set; }
        public Color Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Ellipse; } }

        /// <summary>
        /// Creates a new PrimitiveEllipse.
        /// </summary>
        public PrimitiveEllipse(Point center, Vector majorAxis, Vector normal, double minorAxisRatio, double startAngle, double endAngle, Color color)
        {
            this.Center = center;
            this.MajorAxis = majorAxis;
            this.Normal = normal;
            this.MinorAxisRatio = minorAxisRatio;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Color = color;
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on a circle.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, Vector normal, Color color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, 0.0, 360.0, color)
        {
        }

        /// <summary>
        /// Creates a new PrimitiveEllipse based on an arc.
        /// </summary>
        public PrimitiveEllipse(Point center, double radius, double startAngle, double endAngle, Vector normal, Color color)
            : this(center, Vector.RightVectorFromNormal(normal) * radius, normal, 1.0, startAngle, endAngle, color)
        {
        }
    }
}
