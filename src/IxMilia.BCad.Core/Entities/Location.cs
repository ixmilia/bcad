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

        public Location(Point location, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : this(new PrimitivePoint(location, color), lineTypeSpecification, tag)
        {
        }

        public Location(PrimitivePoint point, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(point.Color, lineTypeSpecification, tag)
        {
            _primitive = point;
            _primitives = new[] { _primitive };
            _snapPoints = new[]
            {
                new EndPoint(Point)
            };
            BoundingBox = new BoundingBox(Point, Vector.Zero);
        }

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings _settings)
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Location Update(
            Optional<Point> point = default,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newPoint = point.HasValue ? point.Value : Point;
            var newColor = color.HasValue ? color.Value : Color;
            var newLineTypeSpecification = lineTypeSpecification.HasValue ? lineTypeSpecification.Value : LineTypeSpecification;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newPoint == Point &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new Location(newPoint, newColor, newLineTypeSpecification, newTag);
        }

        public override string ToString()
        {
            return string.Format("Location: point={0}, color={1}", Point, Color);
        }
    }
}
