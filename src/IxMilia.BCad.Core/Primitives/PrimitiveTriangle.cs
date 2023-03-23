using System;

namespace IxMilia.BCad.Primitives
{
    public class PrimitiveTriangle : IPrimitive
    {
        public PrimitiveKind Kind => PrimitiveKind.Triangle;
        
        public Point P1 { get; }
        public Point P2 { get; }
        public Point P3 { get; }
        public CadColor? Color { get; }

        private Lazy<Point[]> _pointsAsArray;
        private Lazy<PrimitiveLine[]> _boundaryLines;

        public PrimitiveTriangle(Point p1, Point p2, Point p3, CadColor? color)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            Color = color;
            _pointsAsArray = new Lazy<Point[]>(() => new[] { P1, P2, P3 });
            _boundaryLines = new Lazy<PrimitiveLine[]>(() =>
            {
                return new[]
                {
                    new PrimitiveLine(P1, P2, Color),
                    new PrimitiveLine(P2, P3, Color),
                    new PrimitiveLine(P3, P1, Color),
                };
            });
        }

        internal Point[] AsArray() => _pointsAsArray.Value;

        internal PrimitiveLine[] GetBoundaryLines() => _boundaryLines.Value;

        public PrimitiveTriangle Update(
            Optional<Point> p1 = default,
            Optional<Point> p2 = default,
            Optional<Point> p3 = default,
            Optional<CadColor?> color = default)
        {
            var newP1 = p1.GetValue(P1);
            var newP2 = p2.GetValue(P2);
            var newP3 = p3.GetValue(P3);
            var newColor = color.GetValue(Color);
            
            if (newP1 == P1 &&
                newP2 == P2 &&
                newP3 == P3 &&
                newColor == Color)
            {
                return this;
            }

            return new PrimitiveTriangle(newP1, newP2, newP3, newColor);
        }
    }
}
