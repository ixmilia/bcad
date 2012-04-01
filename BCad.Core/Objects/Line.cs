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
        public Point P1 { get; private set; }

        public Point P2 { get; private set; }

        public Color Color { get; private set; }

        public Line(Point p1, Point p2, Color color)
        {
            P1 = p1;
            P2 = p2;
            Color = color;
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

        public Line Update(Point p1 = null, Point p2 = null, Color color = null)
        {
            return new Line(
                p1 ?? this.P1,
                p2 ?? this.P2,
                color ?? this.Color);
        }
    }
}
