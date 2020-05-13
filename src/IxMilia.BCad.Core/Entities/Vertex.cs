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

        public override int GetHashCode()
        {
            return Location.GetHashCode() ^ IncludedAngle.GetHashCode() ^ Direction.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex v)
            {
                return this == v;
            }

            return false;
        }

        public static bool operator==(Vertex a, Vertex b)
        {
            if (ReferenceEquals(a, b))
            {
                // same item
                return true;
            }

            var aIsNull = !(a is object);
            var bIsNull = !(b is object);
            if (aIsNull && bIsNull)
            {
                return true;
            }

            if (aIsNull || bIsNull)
            {
                return false;
            }

            // neither are null at this point
            return a.Location == b.Location
                && a.IncludedAngle == b.IncludedAngle
                && a.Direction == b.Direction;
        }

        public static bool operator!=(Vertex a, Vertex b)
        {
            return !(a == b);
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
