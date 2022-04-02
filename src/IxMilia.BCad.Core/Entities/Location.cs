using System.Collections.Generic;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Location : Entity
    {
        private readonly PrimitivePoint _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public Point Point => _primitive.Location;

        public override EntityKind Kind => EntityKind.Location;

        public override BoundingBox BoundingBox { get; }

        public Location(Point location, CadColor? color = null, object tag = null)
            : this(new PrimitivePoint(location, color), tag)
        {
        }

        public Location(PrimitivePoint point, object tag = null)
            : base(point.Color, tag)
        {
            _primitive = point;
            _primitives = new[] { _primitive };
            _snapPoints = new[]
            {
                new EndPoint(Point)
            };
            BoundingBox = new BoundingBox(Point, Vector.Zero);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Location Update(
            Optional<Point> point = default(Optional<Point>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newPoint = point.HasValue ? point.Value : Point;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newPoint == Point &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Location(newPoint, newColor, newTag);
        }

        public override string ToString()
        {
            return string.Format("Location: point={0}, color={1}", Point, Color);
        }
    }
}
