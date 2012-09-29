namespace BCad.Entities
{
    public class ProjectedCircle : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Ellipse; }
        }

        public Circle OriginalCircle { get; private set; }

        public Point Center { get; private set; }

        public double RadiusX { get; private set; }

        public double RadiusY { get; private set; }

        public double Rotation { get; private set; }

        public ProjectedCircle(Circle circle, Layer layer, Point center, double radiusX, double radiusY, double rotation)
            : base(layer)
        {
            OriginalCircle = circle;
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
        }
    }
}
