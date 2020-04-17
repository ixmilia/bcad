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

        public static IPrimitive PrimitiveFromPointAndVertex(Point lastPoint, Vertex nextVertex)
        {
            if (nextVertex.IsLine)
            {
                return new PrimitiveLine(lastPoint, nextVertex.Location);
            }
            else
            {
                return PrimitiveEllipse.ArcFromPointsAndIncludedAngle(lastPoint, nextVertex.Location, nextVertex.IncludedAngle, nextVertex.Direction);
            }
        }
    }
}
