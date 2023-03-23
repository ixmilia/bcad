using System;
using System.Collections.Generic;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Solid : Entity
    {
        public override EntityKind Kind => EntityKind.Solid;

        private IPrimitive[] _primitives;
        private BoundingBox _boundingBox;
        private SnapPoint[] _snapPoints;
        private Lazy<Point[]> _pointsAsArray;

        public override BoundingBox BoundingBox => _boundingBox;

        public Point P1 { get; }
        public Point P2 { get; }
        public Point P3 { get; }
        public Point P4 { get; }

        public Solid(Point p1, Point p2, Point p3, Point p4, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(color, lineTypeSpecification, tag)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            var t1 = new PrimitiveTriangle(P1, P2, P3, color);
            if (P3 == P4)
            {
                _primitives = new IPrimitive[] { t1 };
                _snapPoints = new SnapPoint[]
                {
                    new EndPoint(P1),
                    new EndPoint(P2),
                    new EndPoint(P3),
                    new MidPoint((P1 + P2) / 2.0),
                    new MidPoint((P2 + P3) / 2.0),
                    new MidPoint((P3 + P1) / 2.0),
                };
            }
            else
            {
                var t2 = new PrimitiveTriangle(P3, P4, P1, color);
                _primitives = new IPrimitive[] { t1, t2 };
                _snapPoints = new SnapPoint[]
                {
                    new EndPoint(P1),
                    new EndPoint(P2),
                    new EndPoint(P3),
                    new EndPoint(P4),
                    new MidPoint((P1 + P2) / 2.0),
                    new MidPoint((P2 + P3) / 2.0),
                    new MidPoint((P3 + P4) / 2.0),
                    new MidPoint((P4 + P1) / 2.0),
                };
            }

            _boundingBox = BoundingBox.FromPoints(P1, P2, P3, P4);
            _pointsAsArray = new Lazy<Point[]>(() => new[] { P1, P2, P3, P4 });
        }

        public Point[] AsPointArray() => _pointsAsArray.Value;

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings settings) => _primitives;

        public override IEnumerable<SnapPoint> GetSnapPoints() => _snapPoints;

        public Solid Update(
            Optional<Point> p1 = default,
            Optional<Point> p2 = default,
            Optional<Point> p3 = default,
            Optional<Point> p4 = default,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newP1 = p1.GetValue(P1);
            var newP2 = p2.GetValue(P2);
            var newP3 = p3.GetValue(P3);
            var newP4 = p4.GetValue(P4);
            var newColor = color.GetValue(Color);
            var newLineTypeSpecification = lineTypeSpecification.GetValue(LineTypeSpecification);
            var newTag = tag.GetValue(Tag);

            if (newP1 == P1 &&
                newP2 == P2 &&
                newP3 == P3 &&
                newP4 == P4 &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new Solid(newP1, newP2, newP3, newP4, newColor, newLineTypeSpecification, newTag);
        }
    }
}
