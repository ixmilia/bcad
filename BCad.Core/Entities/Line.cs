using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Line : Entity
    {
        private const string P1Text = "P1";
        private const string P2Text = "P2";
        private readonly Point p1;
        private readonly Point p2;
        private readonly IndexedColor color;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point P1 { get { return p1; } }

        public Point P2 { get { return p2; } }

        public IndexedColor Color { get { return color; } }

        public Line(Point p1, Point p2, IndexedColor color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.color = color;

            this.primitives = new[] { new PrimitiveLine(P1, P2, Color) };
            this.snapPoints = new SnapPoint[]
            {
                new EndPoint(P1),
                new EndPoint(P2),
                new MidPoint((P1 + P2) / 2.0)
            };
            this.boundingBox = BoundingBox.FromPoints(P1, P2);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case P1Text:
                    return P1;
                case P2Text:
                    return P2;
                case ColorText:
                    return Color;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Line; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Line Update(Point p1 = null, Point p2 = null, IndexedColor? color = null)
        {
            return new Line(
                p1 ?? this.P1,
                p2 ?? this.P2,
                color ?? this.Color);
        }

        public override string ToString()
        {
            return string.Format("Line: {0} {1} {2}", P1, P2, Color);
        }
    }
}
