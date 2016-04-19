using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Line : Entity
    {
        private readonly PrimitiveLine _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public Point P1 => _primitive.P1;

        public Point P2 => _primitive.P2;

        public override EntityKind Kind => EntityKind.Line;

        public override BoundingBox BoundingBox { get; }

        public Line(Point p1, Point p2, CadColor? color, object tag = null)
            : this(new PrimitiveLine(p1, p2, color), tag)
        {
        }

        public Line(PrimitiveLine line, object tag = null)
            : base(line.Color, tag)
        {
            _primitive = line;
            _primitives = new[] { _primitive };
            _snapPoints = new SnapPoint[]
            {
                new EndPoint(P1),
                new EndPoint(P2),
                new MidPoint((P1 + P2) / 2.0)
            };
            BoundingBox = BoundingBox.FromPoints(P1, P2);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(P1):
                    return P1;
                case nameof(P2):
                    return P2;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public Line Update(
            Optional<Point> p1 = default(Optional<Point>),
            Optional<Point> p2 = default(Optional<Point>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newP1 = p1.HasValue ? p1.Value : P1;
            var newP2 = p2.HasValue ? p2.Value : P2;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newP1 == P1 &&
                newP2 == P2 &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Line(newP1, newP2, newColor, newTag);
        }

        public override string ToString()
        {
            return string.Format("Line: p1={0}, p2={1}, color={2}", P1, P2, Color);
        }
    }
}
