using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public class Line : IObject, IPrimitive
    {
        private readonly Point p1;
        private readonly Point p2;
        private readonly Color color;

        public Point P1 { get { return p1; } }

        public Point P2 { get { return p2; } }

        public Color Color { get { return color; } }

        public Line(Point p1, Point p2, Color color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.color = color;
        }

        public IEnumerable<IPrimitive> GetPrimitives()
        {
            yield return this;
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new EndPoint(P1);
            yield return new EndPoint(P2);
            yield return new MidPoint(((P1 + P2) / 2.0).ToPoint());
        }

        public Line Update(Point p1 = null, Point p2 = null, Color? color = null)
        {
            return new Line(
                p1 ?? this.P1,
                p2 ?? this.P2,
                color ?? this.Color);
        }
    }
}
