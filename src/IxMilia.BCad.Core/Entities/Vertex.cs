using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Entities
{
    public class Vertex
    {
        public Point Location { get; }
        public double IncludedAngle { get; }
        public VertexDirection Direction { get; }
        public bool IsLine => IncludedAngle == 0.0;
        public bool IsArc => IncludedAngle != 0.0;

        public Vertex(Point location)
            : this(location, 0.0, default(VertexDirection))
        {
        }

        public Vertex(Point location, double includedAngle, VertexDirection direction)
        {
            Location = location;
            IncludedAngle = includedAngle;
            Direction = direction;
        }

        public static IPrimitive PrimitiveFromVertices(Vertex last, Vertex next)
        {
            if (last.IsLine)
            {
                return new PrimitiveLine(last.Location, next.Location);
            }
            else
            {
                return PrimitiveEllipse.ArcFromPointsAndIncludedAngle(last.Location, next.Location, last.IncludedAngle, last.Direction);
            }
        }
    }
}
